$(document).ready(function () {
    $('#btnAddComment').on('click', function () {
        var commentText = $('textarea#txtAreaComment').val();
        var reviewID = $('#ReviewID').val();
        addCommentToReview(commentText, reviewID);
    });

    $('#btnLikeReview').on('click', function () {
        likeOrDislikeReviewFunction(1, $('#btnLikeReview').val());
    });

    $('#btnDislikeReview').on('click', function () {
        likeOrDislikeReviewFunction(0, $('#btnDislikeReview').val());
    });

    $('#btnRateReview').on('click', function () {
        var radioValue = $('#rdoRateReview:checked').val();
        var reviewID = $('#ReviewID').val();
        rateReview(radioValue, reviewID);
    });

    $('#rating').on('click', function () {
        var value = $('#rating').val();
        showAllRating(value)
    });

    // Toastr-options
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "500",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
});

function showAllRating(value) {
    $.ajax({
        url: '/Review/GetReviewRatings',
        type: 'POST',
        dataType: 'json',
        data: { reviewID: value },
        success: function (data) {
            console.log(data.succeeded);
            var div = $('#modal-wrapper');
            div.empty();
            if (data.succeeded == 1) {
                $.each(data.ratingsToReview, function (index, review) {
                    if (review.Rating != null) {
                        div.append('<p>' + review.Username + '</p>');
                        div.append('<p>' + 'Betygsatte denna recension med en: ' + review.Rating + ':a');
                    }
                });
            }
            else if (data.succeeded == 2) {
                    Command: toastr["error"]("Fel", "Ett fel uppstod, ladda om sidan och försök igen.");
            }
            else {
                div.append('<p>Finns ingen som betygsatt denna recension.</p>');
            }
        }
    });
}


function rateReview(radioValue, reviewID) {
    $.ajax({
        url: '/Review/RateReview',
        type: 'POST',
        dataType: 'json',
        data: { radioValue: radioValue, reviewID: reviewID },
        success: function (data) {
            if (data.succeeded == 6) {
                Command: toastr["error"]("Fel", "Ett fel uppstod, ladda om sidan och försök igen.");
            }
            else if (data.succeeded > 0) {
                    Command: toastr["success"]("Lyckades", "Du har betygsatt denna recension!");
                $('#rating').text(data.rating);
            }
            else if (data.succeeded == 0) {
                    Command: toastr["warning"]("Fel", "Du har redan betygsatt denna recension.");
            }
        }
    })
}

function addCommentToReview(commentText, reviewID) {
    $.ajax({
        url: '/Review/CreateCommentToReview',
        type: 'POST',
        traditional: true,
        dataType: 'json',
        data: { CommentToAdd: commentText, ReviewID: reviewID },
        success: function (data) {
            if (data.allReviewViewModel != null) {
                $('textarea#txtAreaComment').val('');

                var comments = $('#comment-wrapper');

                var noCommentsText = $('#no-comments-text');

                noCommentsText.remove();

                comments.append('<p>' + data.allReviewViewModel.CreatedByName + ': ' + data.allReviewViewModel.CommentToAdd + ', ' + data.allReviewViewModel.CreatedDate + '</p>')

                Command: toastr["success"]("Lyckades", "Din kommentar är upplagd!")
            }
            else if (data.succeeded == false) {
                    Command: toastr["error"]("Fel", "Ett fel uppstod, ladda om sidan och försök igen.")
            }
            else {
                Command: toastr["error"]("Fel", "Du måste fylla i en kommentar längre än 3 och inte större än 150 tecken.")
            }
        }
    });
}

function likeOrDislikeReviewFunction(likeOrDislike, likeOrDislikeValue) {
    $.ajax({
        url: '/Review/LikeOrDislikeReview',
        type: 'POST',
        traditional: true,
        dataType: 'json',
        data: { likeOrDislike: likeOrDislike, likeOrDislikeValue: likeOrDislikeValue },
        success: function (succeeded) {
            if (succeeded == 2) {
                Command: toastr["error"]("Ogilla", "Du gillar inte denna recension.")
                var value = parseInt($('#disLikesSpan').text());
                var newValue = value + 1;
                $('#disLikesSpan').text(newValue);
            }
            else if (succeeded == 1) {
                    Command: toastr["success"]("Gilla", "Du gillar denna recension!")
                var value = parseInt($('#likesSpan').text());
                var newValue = value + 1;
                $('#likesSpan').text(newValue);
            }
            else if (succeeded == 3) {
                    Command: toastr["error"]("Fel", "Ett fel uppstod, ladda om sidan och försök igen.")
            }
            else {
                Command: toastr["warning"]("Fel", "Tyvärr, du har redan gillat/ogillat denna recension!")
            }
        }
    });
}