using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IOptionService
    {
        string CreateOption(Option option);
        string GetOptionByID(int OptionID, out Option option);
        string GetOptionsByQuestionID(int questionId, out List<Option> options);
        string UpdateOption(Option option);
    }
    public class OptionService : IOptionService
    {

        private readonly EchoosingContext _context;

        public OptionService(EchoosingContext context)
        {
            _context = context;
        }

        public string CreateOption(Option option)
        {
            try
            {
                _context.Options.Add(option);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetOptionByID(int OptionID, out Option option)
        {
            option = null;
            try
            {
                option = _context.Options.FirstOrDefault(e => e.OptionId == OptionID);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetOptionsByQuestionID(int questionId, out List<Option> options)
        {
            options = new List<Option>();
            try
            {
                options = _context.Options.Where(e => e.QuestionId == questionId).ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateOption(Option option)
        {
            try
            {
                _context.Options.Update(option);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
