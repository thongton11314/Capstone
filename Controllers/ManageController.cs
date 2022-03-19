using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CapstoneApp.Models;
using System.Collections.Generic;
using System.Net;

namespace CapstoneApp.Controllers
{

    /**
     * Capstone Project 2022
     * Part of the Microsoft ASP.NET Framework MVC 5
     * This class deals with Manage services for a loggedin users such as change password or profile data
     */
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        //SignInManager is used for tasks/actions Before a user is registered
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        //UserManager is used for tasks/actions After a user is registered

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                  message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.EmailAlertDisabled ? "Email alert deactivated"
                : message == ManageMessageId.ListIsEmpty ? "Grocery list is empty"
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasAlertEnabled = HasNewsAlert(),
                HasItemsList = HasItems(),
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        // GET: /Manage/ViewItemsList
        public async Task<ActionResult> ViewItemsList(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                 message == ManageMessageId.ItemAddedSuccess ? "Your item was added successfully."
               : message == ManageMessageId.ItemRemovedSuccess ? "Your item was removed."
               : message == ManageMessageId.ItemDuplicated ? "Your item already exists"
               : message == ManageMessageId.EmailAlertSuccess ? "Email alert activated for items in list"
               : "";


                //get current signed in user to display their added items
                var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());

                if (currUser != null && currUser.Items != null)
                {
                    //some data clean up for displaying purposes
                    IList<string> currItems = currUser.Items.ToLower().TrimEnd().Split(' ');
                    ViewBag.userName = currUser.Email;
                    ViewBag.items = currItems;
                    return View("ViewItems");
                }

            //something is wrong if we reach here
            return RedirectToAction("Index", new { Message = ManageMessageId.ListIsEmpty }); 
        }

        //
        // GET: /Manage/CreateItemsList
        public ActionResult CreateItemsList()
        {
            return View();
        }

        //
        // POST: /Manage/CreateItemsList
        [HttpPost, ActionName("CreateItemsList")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateItems(CreateItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateItemsList");
            }
            //get current user that is signed in
            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());
            if (currUser != null)
            {
                var currItems = currUser.Items;
                if (currItems != null)
                {
                    //check for duplicated items
                    if (currItems.Contains(model.ItemName))
                    {                     
                        return RedirectToAction("ViewItemsList", new { Message = ManageMessageId.ItemDuplicated });
                    }
                    else
                    {
                        currItems += model.ItemName.ToLower() + " ";
                        currUser.Items = currItems;
                    }
                }
                else
                {
                    currUser.Items += model.ItemName.ToLower() + " ";
                }

                //update items list for currently signed in user
                var result = await UserManager.UpdateAsync(currUser);
                if (result.Succeeded)
                {

                    return RedirectToAction("ViewItemsList", new { Message = ManageMessageId.ItemAddedSuccess });
                }
            }

            //something is wrong if we reach here
            return RedirectToAction("Index", new { Message = ManageMessageId.Error });
        }

        //
        // GET: Manage/DeleteItemsList/milk
        public async Task<ActionResult> DeleteItemsList(string itemName)
        {
            if (itemName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
 
            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());
            if(currUser.Items == null)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            else if (!currUser.Items.Contains(itemName.ToLower()))
            {
                return HttpNotFound();
            }
            else
            {
                ViewBag.Item = itemName;
                return View("DeleteItemsList");
            }
        }

        //
        // POST: Manage/DeleteItemsList/milk
        [HttpPost, ActionName("DeleteItemsList")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string itemName)
        {
            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());
            if (currUser != null && itemName != null)
            {
                var currItems = currUser.Items;
                if (currItems != null && currItems.Contains(itemName))
                {
                    currUser.Items = currItems.Remove(currItems.IndexOf(itemName.ToLower()), itemName.Length + 1);

                    //getting rid of the weird invisible element
                    if(currUser.Items.Length == 0 || currUser.Items == " ")
                    {
                        currUser.Items = null;
                    }
                    //update items list for currently signed in user
                    var result = await UserManager.UpdateAsync(currUser);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ViewItemsList", new { Message = ManageMessageId.ItemRemovedSuccess });
                    }
                }
            }

            //something is wrong if we reach here
            return RedirectToAction("Index", new { Message = ManageMessageId.Error });
        }

        //
        // GET: Manage/EnableItemsAlert/
        public ActionResult EnableItemsAlert()
        {
            return View();
        }

        //
        // POST: Manage/EnableItemsAlert/
        [HttpPost, ActionName("EnableItemsAlert")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableAlert(EnableItemsAlertViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EnableItemsAlert");
            }

            var result = await ProcessItemsForAlert(model.ZipCode);
            if(result)
            {
                return RedirectToAction("ViewItemsList", new { Message = ManageMessageId.EmailAlertSuccess });
            }

            //something is wrong if we reach here
            return RedirectToAction("Index", new { Message = ManageMessageId.Error });
        }

        //
        //internal processing for enabling items email alert (grocery list discount items alerts)
        private async Task<bool> ProcessItemsForAlert(string zipCode)
        {
            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());

            if (zipCode != null && currUser.EmailConfirmed && currUser.Items != null && !currUser.NewssellerSub)
            {
                IList<string> currItems = currUser.Items.ToLower().TrimEnd().Split(' ');
                var homeController = DependencyResolver.Current.GetService<HomeController>();
                homeController.ControllerContext = new ControllerContext(this.Request.RequestContext, homeController);

                //get the info for items in user grocery list
                //info includes: store name and address, each store name and address will be unique, with a list 
                //of associated items and their price for each store
                Dictionary<string, Dictionary<string, double>> itemsResult = homeController.GetItemsInfo(zipCode, currItems);
                if (itemsResult != null)
                {
                    //var jsonString = JsonConvert.SerializeObject(itemsResult);                 

                    //email content
                    var accuDate = DateTime.Today.ToString("D");
                    var userID = User.Identity.GetUserId();
                    var subject = "!-------Weekly Discounted Items News Seller-------!";
                    var content = "<p> *These prices valid starting: " + accuDate + "<br>";
                    foreach(var store in itemsResult.Keys)
                    {
                        int i = 0;
                        content += "-------At " + store + "-------" + "<br>";
                        foreach (var item in itemsResult[store].Keys)//get the overall cheapest price for the specified general item
                        {
                            content += "You can get " + currItems[i] + " for as low as $" + itemsResult[store][item].ToString() + "." + "<br>";
                            i++;
                        }
                    }
                    content += "</p><br> Prices may be subjected to change in the next day.";

                    //send email async
                    await UserManager.SendEmailAsync(userID, subject, content);

                    //these steps are important to make sure a user is marked for "News Seller Email Alert Enabled"
                    currUser.NewssellerSub = true;
                    var result = await UserManager.UpdateAsync(currUser);
                    if (result.Succeeded)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //
        // GET: Manage/DisableItemsAlert/
        public ActionResult DisableItemsAlert()
        {
            return View();
        }

        //
        // POST: Manage/EnableItemsAlert/
        [HttpPost, ActionName("DisableItemsAlert")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableAlert()
        {
       
            //current user attribute
            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());

            //error/bad request and input handling
            if(!currUser.NewssellerSub)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }

            currUser.NewssellerSub = false;
            var result = await UserManager.UpdateAsync(currUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.EmailAlertDisabled });
            }
            
            //something is wrong if we reach here
            return RedirectToAction("Index", new { Message = ManageMessageId.Error });
        }

        /*
        // MAYBE DO LATER, NOT NEEDED NOW (NEED TO FIND A WAY TO EDIT)
        // GET: /Manage/EditItemsList/milk
        public async Task<ActionResult> EditItemsList (string itemName)
        {
            if (itemName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());
            if (!currUser.Items.Contains(itemName.ToLower()))
            {
                return HttpNotFound();
            }
            ViewBag.item = itemName;
            return View();
        }

        //
        // POST: /Manage/EditItemsList/ItemName
        [HttpPost, ActionName("EditItemsList")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditItemsList(string itemName, string newItem)
        {
            var currUser = await UserManager.FindByEmailAsync(User.Identity.GetUserName());
            if (currUser != null)
            {
                var currItems = currUser.Items;
                if (currItems != null && currItems.Contains(itemName))
                {
                    currUser.Items = currItems.Replace(itemName, newItem);
                }
                //update items list for currently signed in user
                var result = await UserManager.UpdateAsync(currUser);
                if (result.Succeeded)
                {
                    return RedirectToAction("ViewItemsList", new { Message = ManageMessageId.ItemEditedSuccess });
                }
            }

            // If we got this far, something failed, redisplay form
            return View("ViewItems");
        }
        */

        //(Service not yet available)
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //(Service not yet available)
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //(Service not yet available)
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //(Service not yet available)
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //(Service not yet available)
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //(Service not yet available)
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        // (Service not yet available)
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        // (Service not yet available)
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // (Service not yet available)
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasNewsAlert()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null && user.NewssellerSub == true)
            {
                return true;
            }
            return false;
        }

        private bool HasItems()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null && user.Items != null)
            {
                return true;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            ItemAddedSuccess,
            ItemRemovedSuccess,
            ItemDuplicated,
            ListIsEmpty,
            EmailAlertSuccess,
            EmailAlertDisabled,
            Error
        }

#endregion
    }
}