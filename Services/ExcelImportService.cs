using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Services
{
    public interface IExcelImportService
    {
		Task<int> ProcessExcelFileAsync(IFormFile excelFile, string examName, int duration, DateTime timeStart, string uId);
    }

    public class ExcelImportService : IExcelImportService
    {
        private readonly EchoosingContext _context;
        private readonly ICodeJoinClassService _codeJoinClassService;
        public ExcelImportService(EchoosingContext context, ICodeJoinClassService codeJoinClassService)
        {
            _context = context;
            _codeJoinClassService = codeJoinClassService;
        }

        public async Task<int> ProcessExcelFileAsync(IFormFile excelFile, string examName, int duration, DateTime timeStart, string uId)
        {
            if (excelFile == null || excelFile.Length == 0)
                throw new ArgumentException("Invalid Excel file.");

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    if (package.Workbook.Worksheets.Count == 0 || package.Workbook.Worksheets[0].Dimension == null)
                        throw new Exception("The uploaded Excel file does not contain any valid data.");

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    int totalQuestions = 0;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        string questionContent = worksheet.Cells[row, 1].Text.Trim();
                        if (!string.IsNullOrEmpty(questionContent))
                        {
                            totalQuestions++;
                        }
                    }

                    var exam = await _context.Exams.FirstOrDefaultAsync(e => e.ExamName == examName);
                    if (exam == null)
                    {
                        exam = new Exam
                        {
                            ExamName = examName,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            CreatedBy = uId,
                            Duration = duration,
                            TimeStart = timeStart,
                            TotalQuestions = totalQuestions
                        };
                        _context.Exams.Add(exam);
                        await _context.SaveChangesAsync();
                    }

                    int examId = exam.ExamId;
                    string codeId = Guid.NewGuid().ToString();

                    ExamCode examCode = new ExamCode
                    {
                        CodeId = codeId,
                        ExamId = examId,
                        Code = _codeJoinClassService.Encode(codeId),
                        CreatedDate = DateTime.Now,
                        TimeStart = exam.TimeStart,
                    };
                    _context.ExamCodes.Add(examCode);
                    await _context.SaveChangesAsync();

                    var questionsToAdd = new List<Question>();
                    var optionsToAdd = new List<Option>();
                    var examQuestionsToAdd = new List<ExamQuestion>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        string questionContent = worksheet.Cells[row, 1].Text.Trim();
                        if (string.IsNullOrEmpty(questionContent)) continue;

                        var newQuestion = new Question
                        {
                            Content = questionContent,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        };

                        _context.Questions.Add(newQuestion);
                        await _context.SaveChangesAsync();

                        int newQuestionId = newQuestion.QuestionId;

                        string[] options = new string[6];
                        for (int i = 0; i < 6; i++)
                        {
                            options[i] = worksheet.Cells[row, i + 2].Text.Trim();
                        }

                        string correctOptions = worksheet.Cells[row, 8].Text.Trim();
                        options = options.Where(o => !string.IsNullOrWhiteSpace(o)).Take(6).ToArray();

                        if (options.Length < 2)
                            throw new Exception($"Question at row {row} must have at least 2 valid options.");

                        var correctIndexes = correctOptions.Split(',')
                            .Select(opt => opt.Trim().ToUpper())
                            .Where(opt => new[] { "A", "B", "C", "D", "E", "F" }.Contains(opt))
                            .Select(opt => "ABCDEF".IndexOf(opt))
                            .Where(index => index >= 0 && index < options.Length)
                            .ToList();

                        if (correctIndexes.Count == 0)
                            throw new Exception($"Question at row {row} must have at least one correct answer.");

                        for (int i = 0; i < options.Length; i++)
                        {
                            optionsToAdd.Add(new Option
                            {
                                QuestionId = newQuestionId,
                                Content = options[i],
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now,
                                IsCorrect = correctIndexes.Contains(i)
                            });
                        }

                        examQuestionsToAdd.Add(new ExamQuestion
                        {
                            ExamId = exam.ExamId,
                            QuestionId = newQuestionId,
                            SortOrder = row - 1,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now
                        });
                    }

                    _context.Options.AddRange(optionsToAdd);
                    await _context.SaveChangesAsync();

                    _context.ExamQuestions.AddRange(examQuestionsToAdd);
                    await _context.SaveChangesAsync();
					return exam.ExamId; // ✅ trả về
				}
            }
        }
    }
}
