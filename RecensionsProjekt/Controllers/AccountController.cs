using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecensionsProjekt.Context;
using RecensionsProjekt.Models;
using System.Data.Entity.Validation;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace RecensionsProjekt.Controllers
{
    // Registrera användare & Logga in metoder.
    public class AccountController : Controller
    {
        #region Login
        public ActionResult Login()
        {
            return View("Login");
        }
        #endregion

        #region UserLogin
        [HttpPost]
        public ActionResult UserLogIn(LoginViewModel userToLogin)
        {
            if (!ModelState.IsValid)
            {
                return View("Login");
            }

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    string userToLoginSalt = db.User.Where(x => x.Username == userToLogin.Username).Select(x => x.Salt).SingleOrDefault();
                    string hashedPassword;
                    using (MD5 md5Hash = MD5.Create())
                    {
                        hashedPassword = GetMd5Hash(md5Hash, userToLogin.Password + userToLoginSalt);
                    }

                    if (db.User.Any(x => x.Username == userToLogin.Username && x.Password == hashedPassword))
                    {
                        string username = userToLogin.Username;
                        Guid userID = db.User.Where(x => x.Username == userToLogin.Username && x.Password == hashedPassword).Select(x => x.Id).SingleOrDefault();
                        FormsAuthentication.SetAuthCookie(username, false);
                        
                        Session["loggedInUser"] = userID;
                        Session["currentUsername"] = username;

                        return RedirectToAction("AllReviews", "Review");
                    }
                    else
                    {
                        ModelState.AddModelError("InvalidCredentialsError", "Du angav fel inloggningsuppgifter, försök igen!");
                    }
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return View("Login");
        }
        #endregion

        #region UserLogOut
        public ActionResult UserLogOut()
        {
            FormsAuthentication.SignOut();
            Session["loggedinUser"] = null;
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Register
        public ActionResult Register()
        {
            return View("Register");
        }
        #endregion

        #region AddNewUser
        [HttpPost]
        public ActionResult AddNewUser(RegisterViewModel newUserToRegister)
        {
            if (!ModelState.IsValid)
            {
                return View("Register");
            }

            try
            {
                using (var db = new HermodsProjektEntities())
                {
                    if (db.User.Any(x => x.Username == newUserToRegister.Username || x.Email == newUserToRegister.Email))
                    {
                        ModelState.AddModelError("AlreadyExistsError", "Användarnamn eller Email-adressen finns redan, vänligen välj en annan!");
                        return View("Register");
                    }

                    // Här hashas lösenordet tillsammans med en salt.
                    string hashSaltPassword;
                    string salt = GenerateSalt();
                    using (MD5 md5Hash = MD5.Create())
                    {
                        hashSaltPassword = GetMd5Hash(md5Hash, newUserToRegister.Password + salt);
                    };

                    Guid newUserID = Guid.NewGuid();

                    User newUser = new User
                    {
                        Id = newUserID,
                        FirstName = newUserToRegister.Firstname,
                        LastName = newUserToRegister.Lastname,
                        Username = newUserToRegister.Username,
                        Password = hashSaltPassword,
                        Email = newUserToRegister.Email,
                        Salt = salt
                    };

                    db.User.Add(newUser);
                    db.SaveChanges();

                    ModelState.Clear();
                }
            }
            catch 
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region GenerateSalt
        private string GenerateSalt()
        {
            // Här genereras salt:en.
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string salt = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return salt;
        }
        #endregion

        #region GetMd5Hash
        public string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
        #endregion
    }
}