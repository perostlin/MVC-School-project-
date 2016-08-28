$(document).ready(function () {
    $('#btnRemoveSelectedReviews').on('click', function () {
        reviewDeleteFunction();
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
            if (succeeded != true) {
                Command: toastr["error"]("Fel", "Måste markera en recension innan du kan tabort!")
            }
            else {
                $.each(selectedReview, function (index, reviewToRemove) {
                    $(reviewToRemove).parent().parent().remove();
                })

                Command: toastr["success"]("Borttagna", "Borttagning lyckades!")
            }
        }
    });
}