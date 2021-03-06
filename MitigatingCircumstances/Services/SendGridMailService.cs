﻿using Microsoft.Extensions.Configuration;
using MitigatingCircumstances.Models;
using MitigatingCircumstances.Models.GoogleEntity;
using MitigatingCircumstances.Repositories.Interface;
using MitigatingCircumstances.Services.Interface;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MitigatingCircumstances.Services
{
    public class SendGridMailService : IMailService
    {
        private readonly SendGridClient _sendGridClient;
        private readonly EmailAddress _fromEmailAddress;
        private readonly INotificationRepository _notificationRepository;

        private readonly string _userNotExistTemplateId;
        private readonly string _extensionRequestCreatedTemplateId;
        private readonly string _extensionCreatedTeacherNotificationTemplateId;
        private readonly string _teacherRequestsMoreInfoTemplateId;

        private readonly string _urlDomain;

        public SendGridMailService(IConfiguration config, INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
            var apiKey = config["Mail:SendGridApiKey"];
            _sendGridClient = new SendGridClient(apiKey);
            _fromEmailAddress = new EmailAddress("no-reply@mail.supporthub.cloud", "Support Hub");

            _userNotExistTemplateId = "d-2837e475bc4b4f9484d21c163d033f59";
            _extensionRequestCreatedTemplateId = "d-9c7d3fb57e9446219a66d12225c9b373";
            _extensionCreatedTeacherNotificationTemplateId = "d-7e225f1fa0534146b35711fe385329be";
            _teacherRequestsMoreInfoTemplateId = "d-c85fc0282a05407285f48016195e4981";

            _urlDomain = "https://supporthub.cloud";
        }

        public async void SendInboundEmailExtensionCreated(string toEmail, string toName, ExtensionRequest extensionRequest)
        {
            var dynamicTemplateData = new
            {
                Recipient = new
                {
                    Name = toName
                },
                Extension = new
                {
                    Id = extensionRequest.Id,
                    Title = extensionRequest.Title,
                    Description = extensionRequest.Description,
                    TeacherAssignedTo = extensionRequest.TutorAssignedTo.Fullname,
                    AttachmentCount = extensionRequest.UploadedDocuments?.Count,
                    LinkUrl = $"{_urlDomain}/Student/Applications"
                }
            };

            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleTemplateEmail(
                _fromEmailAddress, to, _extensionRequestCreatedTemplateId, dynamicTemplateData);

            await _sendGridClient.SendEmailAsync(msg);
        }

        public async void SendInboundEmailDoesntExistEmail(string toEmail, string toName, string extensionRequestTitle)
        {
            var dynamicTemplateData = new
            {
                Recipient = new
                {
                    Name = toName
                },
                Extension = new
                {
                    Title = extensionRequestTitle
                },
                LinkUrl = $"{_urlDomain}"
            };

            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleTemplateEmail(
                _fromEmailAddress, to, _userNotExistTemplateId, dynamicTemplateData); 

            await _sendGridClient.SendEmailAsync(msg);
        }

        /// <summary>
        /// Sends a notification email to the teacher letting them know that a student has created an extension request
        /// </summary>
        /// <param name="teacher"></param>
        /// <param name="student"></param>
        /// <param name="extensionRequest"></param>
        public async void SendExtensionCreatedEmailToTeacher(ApplicationUser teacher, 
            ApplicationUser student, ExtensionRequest extensionRequest)
        {
            var dynamicTemplateData = new
            {
                Recipient = new
                {
                    Name = teacher.Firstname
                },
                Extension = new
                {
                    Title = extensionRequest.Title,
                    Description = extensionRequest.Description,
                    AttachmentCount = extensionRequest.UploadedDocuments?.Count,
                    CreatedBy = extensionRequest.StudentCreatedBy.Fullname,
                    LinkUrl = $"{_urlDomain}/Tutor/ReviewExtension?id={extensionRequest.Id}",
                }
            };

            var to = new EmailAddress(teacher.Email, teacher.Fullname);
            var msg = MailHelper.CreateSingleTemplateEmail(
                _fromEmailAddress, to, _extensionCreatedTeacherNotificationTemplateId, dynamicTemplateData);

            await _sendGridClient.SendEmailAsync(msg);

            var notification = new Notification
            {
                UserId = extensionRequest.TutorAssignedTo.Id,
                Text = $"{extensionRequest.StudentCreatedBy.Fullname} has created a new extension {extensionRequest.Title}"
            };

            _notificationRepository.InsertNotification(notification);
        }

        public async void SendTeacherRequestMoreInfoEmail(ApplicationUser teacher,
            ApplicationUser student, ExtensionRequest extensionRequest, string requestMessage)
        {
            var dynamicTemplateData = new
            {
                Student = new
                {
                    Name = student.Firstname
                },
                Teacher = new
                {
                    Name = teacher.Fullname
                },
                Extension = new
                {
                    Title = extensionRequest.Title
                },
                Request = new
                {
                    Message = requestMessage
                }
            };

            var to = new EmailAddress(student.Email, student.Fullname);
            var msg = MailHelper.CreateSingleTemplateEmail(
                _fromEmailAddress, to, _teacherRequestsMoreInfoTemplateId, dynamicTemplateData);

            var notification = new Notification
            {
                UserId = extensionRequest.StudentCreatedBy.Id,
                Text = $"{extensionRequest.TutorAssignedTo.Firstname} requires more information about your extension {extensionRequest.Title}"
            };

            _notificationRepository.InsertNotification(notification);

            await _sendGridClient.SendEmailAsync(msg);
        }

        public void SendExtensionRequestChangeStateEmail(ExtensionRequest extensionRequest)
        {
            var notification = new Notification
            {
                UserId = extensionRequest.StudentCreatedBy.Id,
                Text = $"{extensionRequest.Title} state has been changed to {extensionRequest.Status.ToString()}"
            };

            _notificationRepository.InsertNotification(notification);
        }
    }
}