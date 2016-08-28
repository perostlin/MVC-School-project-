using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecensionsProjekt.Context;
using RecensionsProjekt.Models;
using System.Security.Cryptography;
using System.Web.Security;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace RecensionsProjekt.Controllers
{
    public class UserController : Controller
    {
        #region CurrentUser Methods

        #region GetCurrentUser
        public ActionResult Index()
        {
            if (Session["loggedInUser"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Guid currentUserID = (Guid)Session["loggedInUser"];

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    var selectedUser = db.User.Where(x => x.Id == currentUserID).SingleOrDefault();

                    UserViewModel user = new UserViewModel
                    {
                        Firstname = selectedUser.FirstName,
                        Lastname = selectedUser.LastName,
                        Username = selectedUser.Username,
                        Email = selectedUser.Email,
                        ProfileImagePath = currentUserID.ToString() + ".png"
                    };

                    return View(user);
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        #endregion

        #region EditCurrentUser
        public ActionResult EditUser()
        {
            UserViewModel userViewModelToReturn = null;

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    Guid currentUserID;
                    if (Session["loggedInUser"] != null)
                    {
                        currentUserID = (Guid)Session["loggedInUser"];
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    User currentUser = db.User.Where(x => x.Id == currentUserID).SingleOrDefault();

                    userViewModelToReturn = new UserViewModel
                    {
                        Firstname = currentUser.FirstName,
                        Lastname = currentUser.LastName,
                        Email = currentUser.Email,
                        ProfileImagePath = currentUser.Id.ToString() + ".png",
                        Username = currentUser.Username
                    };
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return View(userViewModelToReturn);
        }

        // Spara ändringar
        [HttpPost]
        public ActionResult UserSaveChanges(UserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                EditUser();
                return View("EditUser");
            }
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    Guid currentUserID = (Guid)Session["loggedInUser"];

                    User userToUpdate = db.User.Where(x => x.Id == currentUserID).SingleOrDefault();

                    if (db.User.Any(x => x.Username == userViewModel.Username && x.Id != currentUserID || x.Email == userViewModel.Email && x.Id != currentUserID))
                    {
                        {
                            ModelState.AddModelError("AlreadyExistsError", "Användarnamn eller Email-adressen finns redan, vänligen välj en annan!");
                            return View("EditUser");
                        }
                    }

                    userToUpdate.FirstName = userViewModel.Firstname;
                    userToUpdate.LastName = userViewModel.Lastname;
                    userToUpdate.Username = userViewModel.Username;
                    userToUpdate.Email = userViewModel.Email;
                    userToUpdate.Username = userViewModel.Username;

                    Session["currentUsername"] = userToUpdate.Username;

                    db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index", "User");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        #endregion

        #region ChangeCurrentUserPassword
        public ActionResult EditPassword()
        {
            if (Session["loggedInUser"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("EditPassword");
        }

        [HttpPost]
        public ActionResult ChangeUserPassword(PasswordViewModel passwordViewModel)
        {
            if(!ModelState.IsValid)
            {
                return View("EditPassword");
            }
            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    Guid currentUserID = (Guid)Session["loggedInUser"];
                    User userNewPassword = db.User.Where(x => x.Id == currentUserID).SingleOrDefault();
                    string hashedPassword;
                    AccountController aController = new AccountController();
                    using (MD5 md5Data = MD5.Create())
                    {
                        hashedPassword = aController.GetMd5Hash(md5Data, passwordViewModel.OldPassword + userNewPassword.Salt);
                    }

                    if (db.User.Any(x => x.Id == currentUserID && x.Password == hashedPassword))
                    {
                        string newHashedPassword;
                        using (MD5 md5Data = MD5.Create())
                        {
                            newHashedPassword = aController.GetMd5Hash(md5Data, passwordViewModel.NewPassword + userNewPassword.Salt);
                        }
                        userNewPassword.Password = newHashedPassword;
                    }
                    else
                    {
                        ModelState.AddModelError("InvalidCredentialsError", "Du angav fel nuvarande lösenord, försök igen!");
                        return View("EditPassword");
                    }

                    db.Entry(userNewPassword).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "User");
        }
        #endregion

        #region RemoveCurrentUser
        [HttpPost]
        public ActionResult RemoveCurrentUser()
        {
            Guid currentUserID = (Guid)Session["loggedInUser"];

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    User userToRemove = db.User.Where(x => x.Id == currentUserID).SingleOrDefault();

                    var reviewsToRemoveList = db.Review.Where(x => x.CreatorUserId == currentUserID).ToList();

                    foreach (var review in reviewsToRemoveList)
                    {
                        var commentToReviewRemoveList = db.CommentToReview.Where(x => x.UserId == currentUserID).ToList();

                        foreach (var commentToReview in commentToReviewRemoveList)
                        {
                            db.CommentToReview.Remove(commentToReview);
                        }

                        var userToReviewRemoveList = db.UserToReview.Where(x => x.UserId == currentUserID).ToList();

                        foreach (var userToReview in userToReviewRemoveList)
                        {
                            db.UserToReview.Remove(userToReview);
                        }

                        db.Review.Remove(review);
                    }

                    var commentsByUserList = db.CommentToReview.Where(x => x.UserId == currentUserID).ToList();

                    foreach (var commentsByUser in commentsByUserList)
                    {
                        db.CommentToReview.Remove(commentsByUser);
                    }

                    var likesOrDislikesByUserList = db.UserToReview.Where(x => x.UserId == currentUserID).ToList();

                    foreach (var likesOrDislikesByUser in likesOrDislikesByUserList)
                    {
                        db.UserToReview.Remove(likesOrDislikesByUser);
                    }

                    db.User.Remove(userToRemove);

                    db.SaveChanges();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            string fullPath = Request.MapPath("~/Images/ProfileImages/" + currentUserID + ".png");
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            FormsAuthentication.SignOut();
            Session["loggedInUser"] = null;
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region UploadToCurrentUser
        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    //string fileExtension = Path.GetExtension(file.FileName);
                    string fileName = Session["loggedInUser"].ToString() + ".png";

                    var path = Path.Combine(Server.MapPath("~/Images/ProfileImages/"), fileName);
                    file.SaveAs(path);
                }
            }

            return RedirectToAction("EditUser", "User");
        }
        #endregion

        #endregion

        #region OtherUser Methods

        public ActionResult OtherUser(Guid? creatorUserID)
        {
            if (creatorUserID == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    var userToReturn = db.User.Where(x => x.Id == creatorUserID).Select(x => new UserViewModel
                    {
                        Username = x.Username,
                        Firstname = x.FirstName,
                        Lastname = x.LastName,
                        Email = x.Email,
                        ProfileImagePath = x.Id.ToString() + ".png"

                    }).SingleOrDefault();

                    var userReviews = db.Review.Where(x => x.CreatorUserId == creatorUserID).ToList();
                    foreach (var item in userReviews)
                    {
                        ReviewViewModel reviewViewModelToAdd = new ReviewViewModel
                        {
                            ReviewID = item.Id,
                            Title = item.Title,
                            Description = item.Description,
                            CreatedDate = item.CreatedDate.ToShortDateString(),
                            TypeOfReviewValue = item.Type,
                            UserRating = item.UserRating
                        };

                        userToReturn.ReviewViewModelList.Add(reviewViewModelToAdd);
                    }

                    return View(userToReturn);
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        #endregion

        #region UserSearch Methods
        [HttpPost]
        public JsonResult FillWithUsers()
        {
            List<string> usernameList = null;
            int succeeded = 0;

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    usernameList = db.User.Select(x => x.Username).ToList();
                }
            }
            catch (Exception)
            {
                return Json(new { succeeded, usernameList });
            }

            if(usernameList != null)
            {
                succeeded = 1;
                return Json(new { succeeded, usernameList });
            }

            succeeded = 2;
            return Json(new { succeeded, usernameList });
        }

        [HttpPost]
        public JsonResult GoToSearchedUser(string username)
        {
            string currentUsername = Session["currentUsername"].ToString();
            Guid searchedUserID = Guid.Empty;
            int succeeded = 0;

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    if (db.User.Any(x => x.Username == username))
                    {
                        succeeded = 1;
                        searchedUserID = db.User.Where(x => x.Username == username).Select(x => x.Id).SingleOrDefault();
                    }
                }
            }
            catch
            {
                succeeded = 2;
                return Json(new { succeeded, currentUsername, searchedUserID });
            }

            return Json(new
            {
                currentUsername,
                searchedUserID,
                succeeded
            });
        }
        #endregion
    }
}