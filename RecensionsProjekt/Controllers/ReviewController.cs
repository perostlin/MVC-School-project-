using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecensionsProjekt.Context;
using RecensionsProjekt.Models;
using System.IO;

namespace RecensionsProjekt.Controllers
{
    public class ReviewController : Controller
    {
        #region CurrentUserReviews
        public ActionResult Index()
        {
            if (Session["loggedInUser"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Guid currentUserID = (Guid)Session["loggedInUser"];
            List<Review> allReviews = new List<Review>();
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    allReviews = db.Review.Where(x => x.CreatorUserId == currentUserID).ToList();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            List<ReviewViewModel> reviewViewModelList = new List<ReviewViewModel>();
            foreach (Review review in allReviews)
            {
                ReviewViewModel newReviewToAdd = new ReviewViewModel
                {
                    ReviewID = review.Id,
                    Title = review.Title,
                    CreatedDate = review.CreatedDate.ToShortDateString(),
                    Description = review.Description,
                    TypeOfReviewValue = review.Type,
                    UserRating = review.UserRating
                };

                reviewViewModelList.Add(newReviewToAdd);
            }
            return View(reviewViewModelList);
        }
        #endregion

        #region RemoveCurrentUserReviews
        public JsonResult RemoveSelectedReviews(List<Guid> selectedReviewsIDs)
        {
            bool succeeded = false;
            if (selectedReviewsIDs != null)
            {
                try
                {
                    succeeded = true;
                    using (var db = new HermodsProjektEntities())
                    {
                        foreach (var id in selectedReviewsIDs)
                        {
                            Review reviewToRemove = db.Review.Where(x => x.Id == id).SingleOrDefault();

                            db.Review.Remove(reviewToRemove);
                            db.SaveChanges();

                            string fullPath = Request.MapPath("~/Images/ReviewImages/" + id + ".png");
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                    }
                }
                catch
                {
                    return Json(succeeded);
                }
            }

            return Json(succeeded);
        }
        #endregion

        #region CreateNewCurrentUserReview
        public ActionResult NewReview()
        {
            if(Session["loggedInUser"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ReviewViewModel reviewViewModel = new ReviewViewModel();
            return View(reviewViewModel);
        }

        [HttpPost]
        public ActionResult CreateNewReview(ReviewViewModel reviewViewModel)
        {
            if (!ModelState.IsValid || reviewViewModel.UserRating == 0)
            {
                ModelState.AddModelError("InvalidRating", "Du måste välja ett betyg!");
                ReviewViewModel theModelToReturn = new ReviewViewModel();
                return View("NewReview", theModelToReturn);
            }

            if (Session["loggedInUser"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Guid currentUserID = (Guid)Session["loggedInUser"];

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    Review newReview = new Review
                    {
                        Id = Guid.NewGuid(),
                        CreatorUserId = currentUserID,
                        Title = reviewViewModel.Title,
                        Description = reviewViewModel.Description,
                        Type = reviewViewModel.TypeOfReviewValue,
                        UserRating = reviewViewModel.UserRating,
                        CreatedDate = DateTime.Now
                    };

                    Upload(newReview.Id);

                    db.Review.Add(newReview);
                    db.SaveChanges();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "Review");
        }
        #endregion

        #region EditCurrentUserReview
        [HttpGet]
        public ActionResult EditReview(Guid? reviewID)
        {
            if (reviewID == null)
            {
                return RedirectToAction("Index", "Review");
            }
            else
            {
                try
                {
                    using (var db = new HermodsProjektEntities())
                    {
                        var reviewToEdit = db.Review.Where(x => x.Id == reviewID).Select(x => new ReviewViewModel
                        {
                            Title = x.Title,
                            Description = x.Description,
                            TypeOfReviewValue = x.Type,
                            UserRating = x.UserRating,
                            ReviewID = x.Id,
                            ReviewImagePath = reviewID.ToString() + ".png"
                        }).SingleOrDefault();

                        return View(reviewToEdit);
                    }
                }
                catch
                {
                    return RedirectToAction("Error", "Home");
                }
            }
        }

        [HttpPost]
        public ActionResult EditSelectedReview(ReviewViewModel reviewViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    var reviewToUpdate = db.Review.Where(x => x.Id == reviewViewModel.ReviewID).SingleOrDefault();

                    reviewToUpdate.Title = reviewViewModel.Title;
                    reviewToUpdate.Description = reviewViewModel.Description;
                    reviewToUpdate.Type = reviewViewModel.TypeOfReviewValue;
                    reviewToUpdate.UserRating = reviewViewModel.UserRating;

                    db.SaveChanges();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "Review");
        }
        #endregion

        #region ShowAllReviews
        public ActionResult AllReviews()
        {
            if (Session["loggedInUser"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Guid currentUserID = (Guid)Session["loggedInUser"];
            List<Review> allReviews = new List<Review>();
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    allReviews = db.Review.ToList();                    

                    if (allReviews.Count != 0)
                    {
                        List<AllReviewViewModel> allReviewViewModelList = new List<AllReviewViewModel>();
                        foreach (Review review in allReviews)
                        {
                            var userInfo = db.User.Where(x => x.Id == review.CreatorUserId).SingleOrDefault();

                            List<UserToReview> userToReviewList = db.UserToReview.Where(x => x.ReviewId == review.Id).ToList();

                            decimal totalRating = (decimal)CalculateReviewRating(userToReviewList);

                            AllReviewViewModel newAllReviewToAdd = new AllReviewViewModel
                            {
                                ReviewID = review.Id,
                                CreatorUserID = userInfo.Id,
                                CreatedByName = userInfo.Username,
                                Title = review.Title,
                                Description = review.Description,
                                CreatedDate = review.CreatedDate.ToShortDateString(),
                                TypeOfReviewValue = review.Type,
                                UserRating = review.UserRating,
                                Likes = review.LikeCount,
                                DisLikes = review.DislikeCount,
                                HasProfilePicture = ConfirmProfilePicture(userInfo.Id),
                                TotalRating = totalRating
                            };

                            allReviewViewModelList.Add(newAllReviewToAdd);
                        }

                        return View(allReviewViewModelList);
                    }
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return View();
        }
        #endregion

        #region ShowReview
        [HttpGet]
        public ActionResult ShowReview(Guid? reviewID)
        {
            if (reviewID == null)
            {
                return RedirectToAction("AllReviews", "Review");
            }
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    decimal reviewRating = (decimal)CalculateReviewRating(db.UserToReview.Where(x => x.ReviewId == reviewID).ToList());

                    var reviewToShow = db.Review.Where(x => x.Id == reviewID).Select(x => new AllReviewViewModel
                    {
                        Title = x.Title,
                        Description = x.Description,
                        TypeOfReviewValue = x.Type,
                        UserRating = x.UserRating,
                        ReviewID = x.Id,
                        CreatorUserID = x.CreatorUserId,
                        CreatedByName = x.User.Username,
                        Likes = x.LikeCount,
                        DisLikes = x.DislikeCount,
                        TotalRating = reviewRating,
                        ReviewImagePath = reviewID.ToString() + ".png"
                    }).SingleOrDefault();

                    var reviewComments = db.CommentToReview.OrderByDescending(x => x.CreatedDate).Where(x => x.ReviewId == reviewID).ToList();
                    foreach (var item in reviewComments)
                    {
                        CommentToReview reviewViewModelToAdd = new CommentToReview
                        {
                            Id = item.Id,
                            ReviewId = item.ReviewId,
                            Comment = item.Comment,
                            CreatedDate = item.CreatedDate,
                            User = db.User.Where(x => x.Id == item.UserId).SingleOrDefault()
                        };

                        reviewToShow.CommentToReviewList.Add(reviewViewModelToAdd);
                    }

                    return View(reviewToShow);
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        #endregion

        #region CommentToReview
        // Skapa kommentar
        public JsonResult CreateCommentToReview(AllReviewViewModel allReviewViewModel)
        {
            bool succeeded = false;

            if (!ModelState.IsValid)
            {
                return Json(allReviewViewModel);
            }

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    Guid userID = (Guid)Session["loggedInUser"];

                    if (db.User.Any(x => x.Id == userID))
                    {
                        var user = db.User.Where(x => x.Id == userID).SingleOrDefault();

                        CommentToReview newCommentToReview = new CommentToReview
                        {
                            Id = Guid.NewGuid(),
                            ReviewId = allReviewViewModel.ReviewID,
                            UserId = (Guid)userID,
                            Comment = allReviewViewModel.CommentToAdd,
                            CreatedDate = DateTime.Now
                        };

                        allReviewViewModel.CreatedByName = user.Username;
                        allReviewViewModel.CreatedDate = DateTime.Now.ToShortDateString();

                        db.CommentToReview.Add(newCommentToReview);
                        succeeded = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        return Json(new { succeeded });
                    }
                }
            }
            catch (Exception)
            {
                return Json(new { succeeded });
            }

            return Json(new { succeeded, allReviewViewModel });
        }
        #endregion

        #region LikeOrDislikeReview
        [HttpPost]
        public JsonResult LikeOrDislikeReview(int likeOrDislike, Guid likeOrDislikeValue)
        {
            int succeeded = 0;
            Guid userID = (Guid)Session["loggedInUser"];

            if (likeOrDislike == 1)
            {
                try
                {
                    using (var db = new HermodsProjektEntities())
                    {
                        if (!db.User.Any(x => x.Id == userID))
                        {
                            succeeded = 3;
                            return Json(succeeded);
                        }

                        bool isTrue = db.UserToReview.Any(x => x.UserId == userID && x.ReviewId == likeOrDislikeValue);
                        var userToReview = db.UserToReview.Where(x => x.UserId == userID && x.ReviewId == likeOrDislikeValue).SingleOrDefault();

                        if (!isTrue)
                        {
                            succeeded = 1;

                            UserToReview userHasLiked = new UserToReview
                            {
                                Id = Guid.NewGuid(),
                                UserId = userID,
                                ReviewId = likeOrDislikeValue,
                                HasLiked = true
                            };

                            var reviewToLike = db.Review.Where(x => x.Id == likeOrDislikeValue).SingleOrDefault();

                            reviewToLike.LikeCount++;

                            db.UserToReview.Add(userHasLiked);
                            db.SaveChanges();
                        }

                        else if (isTrue && userToReview.HasLiked != true)
                        {
                            succeeded = 1;

                            var reviewToLike = db.Review.Where(x => x.Id == likeOrDislikeValue).SingleOrDefault();

                            reviewToLike.LikeCount++;
                            userToReview.HasLiked = true;

                            db.SaveChanges();
                        }

                        else
                        {
                            return Json(succeeded);
                        }
                    }
                }
                catch (Exception)
                {
                    succeeded = 3;
                    return Json(succeeded);
                }
                return Json(succeeded);
            }
            else
            {
                try
                {
                    using (var db = new HermodsProjektEntities())
                    {
                        if (!db.User.Any(x => x.Id == userID))
                        {
                            succeeded = 3;
                            return Json(succeeded);
                        }

                        bool isTrue = db.UserToReview.Any(x => x.UserId == userID && x.ReviewId == likeOrDislikeValue);
                        var userToReview = db.UserToReview.Where(x => x.UserId == userID && x.ReviewId == likeOrDislikeValue).SingleOrDefault();

                        if (!isTrue)
                        {
                            succeeded = 2;

                            UserToReview userHasLiked = new UserToReview
                            {
                                Id = Guid.NewGuid(),
                                UserId = userID,
                                ReviewId = likeOrDislikeValue,
                                HasLiked = true
                            };

                            var reviewToDislike = db.Review.Where(x => x.Id == likeOrDislikeValue).SingleOrDefault();

                            reviewToDislike.DislikeCount++;

                            db.UserToReview.Add(userHasLiked);
                            db.SaveChanges();
                        }

                        else if (isTrue && userToReview.HasLiked != true)
                        {
                            succeeded = 2;

                            var reviewToDislike = db.Review.Where(x => x.Id == likeOrDislikeValue).SingleOrDefault();

                            reviewToDislike.DislikeCount++;
                            userToReview.HasLiked = true;

                            db.SaveChanges();
                        }

                        else
                        {
                            return Json(succeeded);
                        }
                    }
                }
                catch (Exception)
                {
                    succeeded = 4;
                    return Json(succeeded);
                }

                return Json(succeeded);
            }
        }
        #endregion

        #region RateReview
        [HttpPost]
        public JsonResult RateReview(int radioValue, Guid reviewID)
        {
            int succeeded = radioValue;
            Guid userID = (Guid)Session["loggedInUser"];

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    if (!db.User.Any(x => x.Id == userID))
                    {
                        succeeded = 6;
                        return Json(new { succeeded });
                    }

                    bool isTrue = db.UserToReview.Any(x => x.UserId == userID && x.ReviewId == reviewID);
                    var userToReview = db.UserToReview.Where(x => x.UserId == userID && x.ReviewId == reviewID).SingleOrDefault();

                    if (isTrue && userToReview.Rating == null)
                    {
                        userToReview.Rating = radioValue;

                        db.SaveChanges();
                    }
                    else if (!isTrue)
                    {
                        UserToReview newUserserToReview = new UserToReview
                        {
                            Id = Guid.NewGuid(),
                            UserId = userID,
                            ReviewId = reviewID,
                            HasLiked = false,
                            Rating = radioValue
                        };

                        db.UserToReview.Add(newUserserToReview);
                        db.SaveChanges();
                    }
                    else
                    {
                        succeeded = 0;
                    }

                    return Json(new
                    {
                        succeeded,
                        rating = CalculateReviewRating(db.UserToReview.Where(x => x.ReviewId == reviewID).ToList())
                    });
                }
            }
            catch
            {
                succeeded = 6;
                return Json(new { succeeded });
            }
        }
        #endregion

        #region CalculateReviewRating
        public decimal? CalculateReviewRating(List<UserToReview> userToReviewList)
        {
            int reviewCount = userToReviewList.Count;

            var totalRatingSum = reviewCount != 0 ? userToReviewList.Sum(x => x.Rating) / reviewCount : 0;

            return totalRatingSum;
        }
        #endregion

        #region SortAllReviews
        [HttpPost]
        public JsonResult SortAllReviews(int sortValue)
        {
            bool succeeded = false;

            List<AllReviewViewModel> allReviews = new List<AllReviewViewModel>();

            List<Review> reviewToShow = new List<Review>();

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    succeeded = true;

                    reviewToShow = db.Review.ToList();

                    foreach (var item in reviewToShow)
                    {
                        var userInfo = db.User.Where(x => x.Id == item.CreatorUserId).SingleOrDefault();

                        AllReviewViewModel allReviewViewModel = new AllReviewViewModel
                        {
                            Title = item.Title,
                            Description = item.Description,
                            TypeOfReviewValue = item.Type,
                            UserRating = item.UserRating,
                            ReviewID = item.Id,
                            CreatedDate = item.CreatedDate.ToShortDateString(),
                            CreatorUserID = item.CreatorUserId,
                            CreatedByName = item.User.Username,
                            Likes = item.LikeCount,
                            DisLikes = item.DislikeCount,
                            TotalRating = item.ReviewRating,
                            HasProfilePicture = ConfirmProfilePicture(userInfo.Id)
                        };

                        allReviews.Add(allReviewViewModel);
                    }
                }
            }
            catch
            {
                return Json(new { succeeded, allReviews });
            }

            switch (sortValue)
            {
                case 1:
                    allReviews = allReviews.OrderBy(x => x.Title).ToList();
                    break;
                case 2:
                    allReviews = allReviews.OrderBy(x => x.UserRating).ToList();
                    break;
                case 3:
                    allReviews = allReviews.OrderBy(x => x.Likes).ToList();
                    break;
                case 4:
                    allReviews = allReviews.OrderBy(x => x.DisLikes).ToList();
                    break;
                case 5:
                    allReviews = allReviews.OrderBy(x => x.CreatedDate).ToList();
                    break;
                case 6:
                    allReviews = allReviews.OrderBy(x => x.TypeOfReviewValue).ToList();

                    break;
            }

            return Json(new { succeeded, allReviews });
        }
        #endregion

        #region SearchInReviews
        [HttpPost]
        public JsonResult SearchInReviews(string searchValue)
        {
            bool succeeded = false;

            List<AllReviewViewModel> allReviews = new List<AllReviewViewModel>();

            List<Review> reviewToShow = new List<Review>();

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    if (db.Review.Any(x => x.Title == searchValue))
                    {
                        succeeded = true;

                        reviewToShow = db.Review.Where(x => x.Title == searchValue).ToList();

                        foreach (var item in reviewToShow)
                        {
                            var userInfo = db.User.Where(x => x.Id == item.CreatorUserId).SingleOrDefault();

                            AllReviewViewModel allReviewViewModel = new AllReviewViewModel
                            {
                                Title = item.Title,
                                Description = item.Description,
                                TypeOfReviewValue = item.Type,
                                UserRating = item.UserRating,
                                ReviewID = item.Id,
                                CreatedDate = item.CreatedDate.ToShortDateString(),
                                CreatorUserID = item.CreatorUserId,
                                CreatedByName = item.User.Username,
                                Likes = item.LikeCount,
                                DisLikes = item.DislikeCount,
                                TotalRating = item.ReviewRating,
                                HasProfilePicture = ConfirmProfilePicture(userInfo.Id)
                            };

                            allReviews.Add(allReviewViewModel);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return Json(new { succeeded, allReviews });
            }

            return Json(new
            {
                allReviews,
                succeeded
            });
        }
        #endregion

        #region RefreshReviews
        public JsonResult RefreshReviews(string inputValue)
        {
            bool succeeded = false;

            if (inputValue == string.Empty)
            {
                succeeded = true;

                List<AllReviewViewModel> allReviews = new List<AllReviewViewModel>();

                List<Review> reviewToShow = new List<Review>();

                try
                {
                    using (var db = new HermodsProjektEntities())
                    {
                        reviewToShow = db.Review.ToList();

                        foreach (var item in reviewToShow)
                        {
                            var userInfo = db.User.Where(x => x.Id == item.CreatorUserId).SingleOrDefault();

                            AllReviewViewModel allReviewViewModel = new AllReviewViewModel
                            {
                                Title = item.Title,
                                Description = item.Description,
                                TypeOfReviewValue = item.Type,
                                UserRating = item.UserRating,
                                ReviewID = item.Id,
                                CreatedDate = item.CreatedDate.ToShortDateString(),
                                CreatorUserID = item.CreatorUserId,
                                CreatedByName = item.User.Username,
                                Likes = item.LikeCount,
                                DisLikes = item.DislikeCount,
                                TotalRating = item.ReviewRating,
                                HasProfilePicture = ConfirmProfilePicture(userInfo.Id)
                            };

                            allReviews.Add(allReviewViewModel);
                        }
                    }

                    return Json(new { succeeded, allReviews });
                }
                catch
                {
                    return Json(new { succeeded, allReviews });
                }
            }
            else
            {
                return Json(null);
            }
        }
        #endregion

        #region GetReviewRatings
        [HttpPost]
        public JsonResult GetReviewRatings(Guid reviewID)
        {
            int succeeded = 0;

            List<RatingViewModel> ratingsToReview = null;
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    List<UserToReview> userToReviewList = db.UserToReview.Where(x => x.ReviewId == reviewID).ToList();

                    ratingsToReview = new List<RatingViewModel>();

                    foreach (var userToReview in userToReviewList)
                    {
                        RatingViewModel newRatingViewModel = new RatingViewModel
                        {
                            Username = userToReview.User.Username,
                            Rating = userToReview.Rating
                        };

                        ratingsToReview.Add(newRatingViewModel);
                    }
                }
            }
            catch
            {
                succeeded = 2;
                return Json(new { succeeded, ratingsToReview });
            }

            if (ratingsToReview.Count > 0)
            {
                succeeded = 1;
            }

            return Json(new { succeeded, ratingsToReview });
        }
        #endregion

        #region UploadPictureToReview
        [HttpPost]
        public ActionResult Upload(Guid reviewID)
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    //string fileExtension = Path.GetExtension(file.FileName);
                    string fileName = reviewID.ToString() + ".png";

                    var path = Path.Combine(Server.MapPath("~/Images/ReviewImages/"), fileName);
                    file.SaveAs(path);
                }
            }

            return RedirectToAction("Index", "Review");
        }
        #endregion

        #region HelperMethods
        public bool ConfirmProfilePicture(Guid userID)
        {
            bool hasProfilePicture = false;
            if (System.IO.File.Exists(Server.MapPath("~/Images/ProfileImages/" + userID + ".png")))
            {
                hasProfilePicture = true;
            }

            return hasProfilePicture;
        }
        #endregion
    }
}