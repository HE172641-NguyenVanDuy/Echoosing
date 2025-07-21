using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IQuizletService
    {
        public string AddQuizlet(Quizlet quizlet);
        public string AddTerminology(Terminology terminology);
        List<Quizlet> GetQuizlets();

        List<Quizlet> GetQuizletsByUserID(string userID);
        List<Terminology> GetTerminologiesByQuizletId(string quizletId);
        public int CountTerminologiesByQuizletId(string quizletId);
        public string GetUsernameByUserId(string userId);
        public void DeleteQuizlet(string quizletId);
        public string UpdateQuizlet(Quizlet quizlet);
        public string UpdateTerminology(Terminology terminology);
        string UpdateTerminology2(Terminology terminology);
    }
    public class QuizletService : IQuizletService
    {
        private readonly EchoosingContext context;
        public QuizletService()
        {
            context = new EchoosingContext();
        }
        public string AddQuizlet(Quizlet quizlet)
        {
            try
            {
                context.Quizlets.Add(quizlet);
                context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string AddTerminology(Terminology Terminology)
        {
            try
            {
                context.Terminologies.Add(Terminology);
                context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<Quizlet> GetQuizlets()
        {

            return context.Quizlets.ToList();

        }

        public List<Quizlet> GetQuizletsByUserID(string userID)
        {
            return context.Quizlets
                .Where(q => q.UserId == userID && (q.IsDelete == false || q.IsDelete == null))
                .ToList();
        }

        public List<Terminology> GetTerminologiesByQuizletId(string quizletId)
        {
            return context.Terminologies
                .Where(t => t.QuizletId == quizletId && (t.IsDelete == false || t.IsDelete == null))
                .ToList();
        }
        public int CountTerminologiesByQuizletId(string quizletId)
        {
            return context.Terminologies
                .Count(t => t.QuizletId == quizletId && (t.IsDelete == false || t.IsDelete == null));
        }
        public string GetUsernameByUserId(string userId)
        {
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            return user != null ? user.Username : "Unknown User";
        }

        public void DeleteQuizlet(string quizletId)
        {
            var quizlet = context.Quizlets.FirstOrDefault(q => q.QuizletId == quizletId);
            if (quizlet != null)
            {
                quizlet.IsDelete = true;
                context.SaveChanges();
            }
        }
        public string UpdateQuizlet(Quizlet quizlet)
        {
            try
            {
                var existingQuizlet = context.Quizlets.FirstOrDefault(q => q.QuizletId == quizlet.QuizletId);
                if (existingQuizlet != null)
                {
                    existingQuizlet.QuizletName = quizlet.QuizletName;
                    existingQuizlet.UserId = quizlet.UserId;
                    existingQuizlet.IsDelete = quizlet.IsDelete;
                    context.SaveChanges();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateTerminology(Terminology terminology)
        {
            try
            {
                var existingTerminology = context.Terminologies.FirstOrDefault(t => t.TerminologyId == terminology.TerminologyId);
                if (existingTerminology != null)
                {
                    existingTerminology.Academic = terminology.Academic;
                    existingTerminology.Definition = terminology.Definition;

                    context.SaveChanges();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateTerminology2(Terminology terminology)
        {
            try
            {
                var existingTerminology = context.Terminologies.FirstOrDefault(t => t.TerminologyId == terminology.TerminologyId);
                if (existingTerminology != null)
                {
                    existingTerminology.Academic = terminology.Academic;
                    existingTerminology.Definition = terminology.Definition;
                    context.SaveChanges();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
