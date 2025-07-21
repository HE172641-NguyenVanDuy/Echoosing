using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObjects.Models;

public partial class EchoosingContext : DbContext
{
    public EchoosingContext()
    {
    }

    public EchoosingContext(DbContextOptions<EchoosingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassExam> ClassExams { get; set; }

    public virtual DbSet<ClassUser> ClassUsers { get; set; }

    public virtual DbSet<CronJobSentMail> CronJobSentMails { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamAttempt> ExamAttempts { get; set; }

    public virtual DbSet<ExamCode> ExamCodes { get; set; }

    public virtual DbSet<ExamQuestion> ExamQuestions { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quizlet> Quizlets { get; set; }

    public virtual DbSet<Systemkey> Systemkeys { get; set; }

    public virtual DbSet<Terminology> Terminologies { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        if (!optionsBuilder.IsConfigured) { optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnectionString")); }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D4825024FAFE9F42");

            entity.ToTable("Answer", tb => tb.HasComment("Lưu lại phương án trả lời của người dùng trong một lần làm bài."));

            entity.Property(e => e.AnswerId)
                .HasComment("Câu trả lời của người dùng")
                .HasColumnName("AnswerID");
            entity.Property(e => e.AttemptId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("AttemptID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Attempt).WithMany(p => p.Answers)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Answer__AttemptI__2645B050");

            entity.HasOne(d => d.Option).WithMany(p => p.Answers)
                .HasForeignKey(d => d.OptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Answer__OptionID__282DF8C2");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Answer__Question__2739D489");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Class__CB1927A0BE92F735");

            entity.ToTable("Class");

            entity.Property(e => e.ClassId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.CodeJoinClass)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasComment("code join class mã hóa từ classID");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdateDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ClassExam>(entity =>
        {
            entity.HasKey(e => e.ClassExamId).HasName("PK__ClassExa__46F1EFC03BAA9BDE");

            entity.ToTable("ClassExam");

            entity.Property(e => e.ClassExamId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasColumnName("ClassExamID");
            entity.Property(e => e.ClassId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ClassID");
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreateUser)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdateDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassExams)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassExam_Class");

            entity.HasOne(d => d.Exam).WithMany(p => p.ClassExams)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK_ClassExam_Exam");
        });

        modelBuilder.Entity<ClassUser>(entity =>
        {
            entity.HasKey(e => e.ClassUserId).HasName("PK__ClassUse__9CC6CAAFC97DF820");

            entity.ToTable("ClassUser");

            entity.Property(e => e.ClassUserId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ClassUserID");
            entity.Property(e => e.ClassId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ClassID");
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.UpdateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassUsers)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassUser_Class");

            entity.HasOne(d => d.User).WithMany(p => p.ClassUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ClassUser_Users");
        });

        modelBuilder.Entity<CronJobSentMail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CronJobS__3214EC27C365F1A5");

            entity.ToTable("CronJobSentMail");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.ClassId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ClassID");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ErrorLog)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.Status).HasComment("1: Thành công, 2: Thất bại, 0: Chưa gửi ");
            entity.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.Class).WithMany(p => p.CronJobSentMails)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ClassID");

            entity.HasOne(d => d.Exam).WithMany(p => p.CronJobSentMails)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK_ExamID");

            entity.HasOne(d => d.User).WithMany(p => p.CronJobSentMails)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserID");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__Exam__297521A709937A07");

            entity.ToTable("Exam", tb => tb.HasComment("Lưu trữ thông tin về bài kiểm tra."));

            entity.Property(e => e.ExamId)
                .HasComment("Lưu trữ thông tin về bài kiểm tra.")
                .HasColumnName("ExamID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasComment("FK với Users");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Thời gian tạo ra bài kiểm tra")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasComment("Mô tả bài thi");
            entity.Property(e => e.Duration).HasComment("Thời gian làm bài (phút)");
            entity.Property(e => e.ExamName)
                .HasMaxLength(255)
                .HasComment("Tên bài kiểm tra");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasComment("Xóa");
            entity.Property(e => e.IsRetake)
                .HasDefaultValue(false)
                .HasComment("Có cho làm lại bài kiểm tra hay không");
            entity.Property(e => e.TimeStart).HasComment("Thời gian bắt đầu bài kiểm tra");
            entity.Property(e => e.TotalQuestions).HasComment("Tổng số câu hỏi");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("Thời gian update bài kiểm tra")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__ExamAtte__891A688683B8D69E");

            entity.ToTable("ExamAttempt", tb => tb.HasComment("Lịch sử làm bài của người dùng"));

            entity.Property(e => e.AttemptId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())")
                .IsFixedLength()
                .HasComment("Lịch sử làm bài của người dùng")
                .HasColumnName("AttemptID");
            entity.Property(e => e.ClassId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("ClassID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.ExamCode)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasComment("Tham gia kiểm tra bằng code");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasComment("Status lấy trong bảng systemkey [9]");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UserID");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasComment("Tên tham gia bằng code");

            entity.HasOne(d => d.Class).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_ExamAttempt_Class");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamAttem__ExamI__2180FB33");

            entity.HasOne(d => d.User).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ExamAttem__UserI__208CD6FA");
        });

        modelBuilder.Entity<ExamCode>(entity =>
        {
            entity.HasKey(e => e.CodeId).HasName("PK__ExamCode__C6DE2C35502AAB53");

            entity.ToTable("ExamCode", tb => tb.HasComment("Mã Code cho từng đề thi"));

            entity.HasIndex(e => e.Code, "UQ__ExamCode__A25C5AA76E1FDEF8").IsUnique();

            entity.Property(e => e.CodeId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CodeID");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.ExpiryDate)
                .HasComment("Thời gian hết hạn")
                .HasColumnType("datetime");
            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasComment("Đã dùng hay chưa");
            entity.Property(e => e.TimeStart)
                .HasComment("Thời gian mở")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamCodes)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamCode__ExamID__43D61337");
        });

        modelBuilder.Entity<ExamQuestion>(entity =>
        {
            entity.HasKey(e => e.ExamQuestionId).HasName("PK__ExamQues__EFAED8267B184896");

            entity.ToTable("ExamQuestion", tb => tb.HasComment("Xác định danh sách câu hỏi có trong một bài kiểm tra."));

            entity.HasIndex(e => new { e.ExamId, e.QuestionId }, "UQ__ExamQues__F9A9275E84D3AB25").IsUnique();

            entity.Property(e => e.ExamQuestionId).HasColumnName("ExamQuestionID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamQuestions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamQuest__ExamI__19DFD96B");

            entity.HasOne(d => d.Question).WithMany(p => p.ExamQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamQuest__Quest__1AD3FDA4");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Option__92C7A1DFB71071F4");

            entity.ToTable("Option", tb => tb.HasComment("phương án trả lời cho từng câu hỏi."));

            entity.Property(e => e.OptionId)
                .HasComment("Phương án trả lời cho từng câu hỏi.")
                .HasColumnName("OptionID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Option__Question__14270015");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8CFD821A3E");

            entity.ToTable("Question", tb => tb.HasComment("Lưu thông tin về từng câu hỏi trắc nghiệm."));

            entity.Property(e => e.QuestionId)
                .HasComment("Câu hỏi")
                .HasColumnName("QuestionID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Quizlet>(entity =>
        {
            entity.HasKey(e => e.QuizletId).HasName("PK__Quizlet__E71C5488164A208D");

            entity.ToTable("Quizlet");

            entity.Property(e => e.QuizletId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.IsPublic)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.QuizletName).HasMaxLength(255);
            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.User).WithMany(p => p.Quizlets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Quizlet__User__208CD6FA");
        });

        modelBuilder.Entity<Systemkey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__systemke__3214EC2720B63856");

            entity.ToTable("systemkey", tb => tb.HasComment("Lưu trữ các cấu hình chung hoặc danh mục hệ thống."));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CodeKey)
                .HasMaxLength(255)
                .IsUnicode(false)
                .UseCollation("Vietnamese_CI_AS");
            entity.Property(e => e.CodeValue)
                .HasMaxLength(255)
                .IsUnicode(false)
                .UseCollation("Vietnamese_CI_AS");
            entity.Property(e => e.Description).UseCollation("Vietnamese_CI_AS");
            entity.Property(e => e.IsDelete).HasDefaultValue(false);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
        });

        modelBuilder.Entity<Terminology>(entity =>
        {
            entity.HasKey(e => e.TerminologyId).HasName("PK__Terminol__2482D8106FAE6AA9");

            entity.ToTable("Terminology");

            entity.Property(e => e.TerminologyId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.QuizletId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Quizlet).WithMany(p => p.Terminologies)
                .HasForeignKey(d => d.QuizletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Terminology__Quizlet__2180FB33");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC7FF6F848");

            entity.ToTable(tb => tb.HasComment("Thông tin Người dùng"));

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E49BA40AC0").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534DA2CFB24").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasDefaultValueSql("(newid())")
                .IsFixedLength()
                .HasComment("Thông tin Người dùng")
                .HasColumnName("UserID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Otp)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasComment("OTP đổi mật khẩu")
                .HasColumnName("OTP");
            entity.Property(e => e.OtpexpirationTime)
                .HasComment("Thời gian hết hạn OTP")
                .HasColumnType("datetime")
                .HasColumnName("OTPExpirationTime");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasComment("Mật khẩu đã Hash");
            entity.Property(e => e.Role).HasComment("Lấy Role từ bảng systemkey [1]");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
