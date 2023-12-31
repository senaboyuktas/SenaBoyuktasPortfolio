﻿using BusinessLayer.Concrete;
using DataAccessLayer.Concrete;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;
using BusinessLayer.ValidationRules;
using FluentValidation.Results;

namespace Core_Proje.Areas.Writer.Controllers
{
    [Area("Writer")]
    [Route("Writer/[controller]")]
    public class MessageController : Controller
    {
        WriterMessageManager writerMessageManager = new WriterMessageManager(new EfWriterMessageDal());
        private readonly UserManager<WriterUser> _userManager;

        public MessageController(UserManager<WriterUser> userManager)
        {
            _userManager = userManager;
        }

        [Route("Inbox")]
        public async Task<IActionResult> Inbox(string p)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            p = user.Email;
            var MessageList = writerMessageManager.GetListReceiverMessage(p).OrderByDescending(x => x.Date).ToList();

            return View(MessageList);
        }

        [Route("Sendbox")]

        public async Task<IActionResult> Sendbox(string p)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            p = user.Email;
            var MessageList = writerMessageManager.GetListSenderMessage(p).OrderByDescending(x => x.Date).ToList();

            return View(MessageList);
        }

        [Route("InboxMessageDetails/{id}")]

        public IActionResult InboxMessageDetails(int id)
        {
            var values = writerMessageManager.TGetByID(id);
            return View(values);
        }

        [Route("SendboxMessageDetails/{id}")]
        public IActionResult SendboxMessageDetails(int id)
        {
            var values = writerMessageManager.TGetByID(id);
            return View(values);
        }

        [Route("NewMessage")]
        [HttpGet]
        public IActionResult NewMessage()
        {
            return View();
        }

        [Route("NewMessage")]
        [HttpPost]
        public async Task<IActionResult> NewMessage(WriterMessage writerMessage)
        {
            WriterMessageValidator validations = new WriterMessageValidator();
            ValidationResult result = validations.Validate(writerMessage);

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            writerMessage.Date = DateTime.Now;
            writerMessage.Sender = user.Email;
            writerMessage.SenderName = user.Name + " " + user.Surname;

            Context context = new Context();
            var usernamesurname = context.Users.Where(x => x.Email == writerMessage.Receiver).Select(y => y.Name + " " + y.Surname).FirstOrDefault();

            writerMessage.ReceiverName = usernamesurname;

            if (result.IsValid)
            {
                writerMessageManager.TAdd(writerMessage);
                return RedirectToAction("Sendbox");
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
            }
            return View();
        }
    }
}
