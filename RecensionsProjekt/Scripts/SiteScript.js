$(document).ready(function () {
    $('#btnRemoveSelectedReviews').on('click', function () {
        reviewDeleteFunction();
    });

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

    $('#btnSortReviews').on('click', function () {
        var value = $('#sortReviews option:selected').val();
        sortReviews(value);
    });

    $('#btnSearchReview').on('click', function () {
        var value = $('#tbSearchInput').val();
        searchReview(value);
    });

    $('#tbSearchInput').on('input', function () {
        var value = $('#tbSearchInput').val();
        refreshReviews(value);
    });

    $('#rating').on('click', function () {
        var value = $('#rating').val();
        showAllRating(value)
    });
});

function showAllRating(value) {
    $.ajax({
        url: '/Review/GetReviewRatings',
        type: 'POST',
        dataType: 'json',
        data: { reviewID: value },
        success: function (data) {            
            var html = '';
            $.each(data, function (index, review) {
                html += review.Username + ' betygsatte denna recension med en: ' + review.Rating + '<br /><br />';
            });

            Command: toastr["info"](html + '<br /><br /><button type="button" class="btn clear">Stäng</button>', "De som betygsatt är:");
        }
    });
}

function refreshReviews(value) {
    $.ajax({
        url: '/Review/RefreshReviews',
        type: 'POST',
        dataType: 'json',
        data: { inputValue: value },
        success: function (data) {
            printHtml(data);
        }
    });
}

function sortReviews(value) {
    $.ajax({
        url: '/Review/SortAllReviews',
        type: 'POST',
        dataType: 'json',
        data: { sortValue: value },
        success: function (data) {
            printHtml(data);
        }
    });
}

function searchReview(value) {
    $.ajax({
        url: '/Review/SearchInReviews',
        type: 'POST',
        dataType: 'json',
        data: { searchValue: value },
        success: function (data) {
            if (data.succeeded == true) {
                printHtml(data.allReviews);
            }
            else {
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

                Command: toastr["error"]("Misslyckades", "Det fanns ingen recension med det namnet, försök igen.")
            }
        }
    });
}

function printHtml(data) {
    var table = $('#allReviewsTable');
    var body = $('#tableWrapper');

    body.html('');
    $.each(data, function (index, review) {
        var tr = $('<tr></tr>')

        tr.append('<td><label for="' + review.Title + '"/>' + review.Title + '</td>');
        tr.append('<td><label for="' + review.Description + '"/>' + review.Description + '</td>');
        tr.append('<td><label for="' + review.CreatedDate + '"/>' + review.CreatedDate + '</td>');
        tr.append('<td><label for="' + review.UserRating + '"/>' + review.UserRating + '</td>');
        tr.append('<td><label for="' + review.TypeOfReviewValue + '"/>' + review.TypeOfReviewValue + '</td>');
        tr.append('<td><label for="' + review.Likes + '"/>' + review.Likes + '</td>');
        tr.append('<td><label for="' + review.DisLiked + '"/>' + review.DisLikes + '</td>');
        tr.append('<td><a href="/User/OtherUser?CreatorUserID=' + review.CreatorUserID + '">' + review.CreatedByName + '</a></td>');
        tr.append('<td><a href="/Review/ShowReview?ReviewID=' + review.ReviewID + '">Visa</a></td>');
        body.append(tr);

    })

    //table.addClass("table table-bordered table-hover");
    body.addClass("table table-bordered table-hover");
}

function sortReviews(value) {
    $.ajax({
        url: '/Review/SortAllReviews',
        type: 'POST',
        dataType: 'json',
        data: { sortValue: value },
        success: function (data) {
            var table = $('#allReviewsTable');
            var body = $('#tableWrapper');

            body.html('');
            $.each(data, function (index, review) {
                var tr = $('<tr></tr>')

                tr.append('<td><label for="' + review.Title + '"/>' + review.Title + '</td>');
                tr.append('<td><label for="' + review.Description + '"/>' + review.Description + '</td>');
                tr.append('<td><label for="' + review.CreatedDate + '"/>' + review.CreatedDate + '</td>');
                tr.append('<td><label for="' + review.UserRating + '"/>' + review.UserRating + '</td>');
                tr.append('<td><label for="' + review.TypeOfReviewValue + '"/>' + review.TypeOfReviewValue + '</td>');
                tr.append('<td><label for="' + review.Likes + '"/>' + review.Likes + '</td>');
                tr.append('<td><label for="' + review.DisLiked + '"/>' + review.DisLikes + '</td>');
                tr.append('<td><a href="/User/OtherUser?CreatorUserID=' + review.CreatorUserID + '">' + review.CreatedByName + '</a></td>');
                tr.append('<td><a href="/Review/ShowReview?ReviewID=' + review.ReviewID + '">Visa</a></td>');
                body.append(tr);

            })

            //table.addClass("table table-bordered table-hover");
            body.addClass("table table-bordered table-hover");

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

            if (data.succeeded > 0) {
                Command: toastr["success"]("Lyckades", "Du har betygsatt denna recension!")
                $('#rating').text(data.rating);
            }
            else {
                Command: toastr["warning"]("Fel", "Du har redan betygsatt denna recension.")
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
        success: function (allReviewViewModel) {
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

            if (allReviewViewModel != null) {
                $('textarea#txtAreaComment').val('');

                $('#comment-wrapper').append('<p>' + allReviewViewModel.CreatedByName + ': ' + allReviewViewModel.CommentToAdd + ', ' + allReviewViewModel.CreatedDate + '</p>')

                Command: toastr["success"]("Lyckades", "Din kommentar är upplagd!")
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

            if (succeeded == 2) {
                Command: toastr["error"]("Ogilla", "Du gillar inte denna recension.")
            }
            else if (succeeded == 1) {
                    Command: toastr["success"]("Gilla", "Du gillar denna recension!")
            }
            else {
                Command: toastr["warning"]("Fel", "Tyvärr, du har redan gillat/ogillat denna recension!")
            }
        }
    });
}

function reviewDeleteFunction() {
    var r = confirm("Vill du verkligen tabort vald eller valda recensioner?");
    if (r == true) {
        var selectedReview = new Array();
        $('.selectedReviewItems:checked').each(function () {
            selectedReview.push($(this));
        });

        RemoveSelectedReviews(selectedReview);
    }
}

function RemoveSelectedReviews(selectedReview) {
    var array = [];
    $.each(selectedReview, function (index, review) {
        array.push($(review).val());
    })

    $.ajax({
        url: '/Review/RemoveSelectedReviews',
        type: 'POST',
        traditional: true,
        dataType: 'json',
        data: { selectedReviewsIDs: array },
        success: function (succeeded) {
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

            if (succeeded != true) {
                Command: toastr["error"]("Fel", "Måste markera en recension innan du kan tabort!")
            }
            else {
                console.log(selectedReview);
                $.each(selectedReview, function (index, reviewToRemove) {
                    $(reviewToRemove).parent().parent().remove();
                })

                Command: toastr["success"]("Borttagna", "Borttagning lyckades!")
            }
        }
    });
}