using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAccountService
    {
        string GetUserIDLogin(HttpContext context);
        string GetUserRoleLogin(HttpContext context);
        string UpdateUserLogin(User user);
        string GetUserByUserID(string UserID, out User user);
    }

    public class AccountService : IAccountService
    {
        private readonly EchoosingContext _context;
        public AccountService(EchoosingContext context)
        {
            _context = context;
        }
        public string GetUserIDLogin(HttpContext context)
        {
            string userID = "";
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                userID = context.User.FindFirst("UserID")?.Value ?? "";
            }
            return userID;
        }

        public string GetUserRoleLogin(HttpContext context)
        {
            string userRole = "";
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                userRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "3";
            }
            return userRole;
        }

        public string UpdateUserLogin(User user)
        {
            try
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetUserByUserID(string UserID, out User user)
        {
            user = null;
            try
            {
                user = _context.Users.FirstOrDefault(u => u.UserId == UserID);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
