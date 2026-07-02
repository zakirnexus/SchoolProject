using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Models.Colleges;
using SchoolProject.Models.Courses;
using SchoolProject.Models.Lookups;
using SchoolProject.Models.StudyAbroad;

namespace SchoolProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // SCHOOLS
        public DbSet<School> Schools { get; set; }
        public DbSet<Syllabus> Syllabuses { get; set; }
        public DbSet<SeoContent> SeoContents { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<DynamicContents> DynamicContents { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<SchoolSyllabus> SchoolSyllabuses { get; set; }

        // SHARED LOOKUPS
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Coed> Coeds { get; set; }
        public DbSet<InstOwnership> InstOwnerships { get; set; }
        public DbSet<Nsewc> Nsewcs { get; set; }

        // COLLEGES
        public DbSet<CourseLevel> CourseLevels { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Course> Courses { get; set; }
		public DbSet<DegreeType> DegreeTypes { get; set; }
		public DbSet<Subject> Subjects { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<CollegeCourse> CollegeCourses { get; set; }
        public DbSet<CollegeEnquiry> CollegeEnquiries { get; set; }
        public DbSet<SeoContentCollege> SeoContentColleges { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<CoursePage> CoursePages { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        // STUDY ABROAD (International)
        public DbSet<Country> IntlCountries { get; set; }           // maps to tb_country
        public DbSet<IntlState> IntlStates { get; set; }            // maps to tb_intl_states
        public DbSet<IntlCity> IntlCities { get; set; }             // maps to tb_intl_cities
        public DbSet<IntlInstitute> IntlInstitutes { get; set; }    // maps to tb_intl_institutes
        public DbSet<CourseCategoryIntl> IntlCourseCategories { get; set; } // tb_intl_CourseCategory
        public DbSet<CourseNameIntl> IntlCourseNames { get; set; }
        public DbSet<CourseIntl> IntlCourses { get; set; }          // tb_intl_Courses
        public DbSet<InstituteCourse> IntlInstituteCourses { get; set; }    // tb_intl_InstituteCourse
        public DbSet<CourseLevelIntl> IntlCourseLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SCHOOLS CONFIG
            modelBuilder.Entity<School>()
                .HasOne(s => s.Coed)
                .WithMany(c => c.Schools)
                .HasForeignKey(s => s.CoedId);

            modelBuilder.Entity<School>()
                .HasOne(s => s.Locality)
                .WithMany(l => l.Schools)
                .HasForeignKey(s => s.LocalityId);

            modelBuilder.Entity<School>()
                .HasOne(s => s.NsewcNav)
                .WithMany(n => n.Schools)
                .HasForeignKey(s => s.NsewcId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Locality>()
                .HasOne(l => l.NsewcNav)
                .WithMany(n => n.Localities)
                .HasForeignKey(l => l.NsewcId)
                .OnDelete(DeleteBehavior.SetNull);

            // STANDARDIZED: School ownership FK uses InstOwnershipId
            modelBuilder.Entity<School>()
                .HasOne(s => s.Ownership)
                .WithMany()
                .HasForeignKey(s => s.InstOwnershipId);

            modelBuilder.Entity<SchoolSyllabus>()
                .ToTable("school_syllabuses")
                .HasIndex(ss => new { ss.InstituteId, ss.SyllabusId })
                .IsUnique();

            modelBuilder.Entity<SchoolSyllabus>()
                .HasOne(ss => ss.School)
                .WithMany(s => s.SchoolSyllabuses)
                .HasForeignKey(ss => ss.InstituteId);

            modelBuilder.Entity<SchoolSyllabus>()
                .HasOne(ss => ss.Syllabus)
                .WithMany()
                .HasForeignKey(ss => ss.SyllabusId);

            // COLLEGES CONFIG
            modelBuilder.Entity<College>()
                .HasOne(c => c.City)
                .WithMany()
                .HasForeignKey(c => c.CityId);

            modelBuilder.Entity<College>()
                .HasOne(c => c.Locality)
                .WithMany(l => l.Colleges)
                .HasForeignKey(c => c.LocalityId);

            modelBuilder.Entity<College>()
                .HasOne(c => c.Coed)
                .WithMany()
                .HasForeignKey(c => c.CoedId);

            // STANDARDIZED: College ownership FK uses InstOwnershipId
            modelBuilder.Entity<College>()
                .HasOne(c => c.Ownership)
                .WithMany()
                .HasForeignKey(c => c.InstOwnershipId);

            modelBuilder.Entity<College>()
                .HasOne(c => c.State)
                .WithMany()
                .HasForeignKey(c => c.StateId);

            modelBuilder.Entity<CollegeCourse>()
                .HasOne(cc => cc.College)
                .WithMany(c => c.CollegeCourses)
                .HasForeignKey(cc => cc.InstituteId);

            modelBuilder.Entity<CollegeCourse>()
                .HasOne(cc => cc.Course)
                .WithMany()
                .HasForeignKey(cc => cc.CourseId);

            // FIX: Use SpecializationId (int?) instead of Specialization (string) for unique index
            // This avoids CS1061 if Specialization string property is missing from compiled DLL
            modelBuilder.Entity<CollegeCourse>()
                .HasIndex(cc => new { cc.InstituteId, cc.CourseId, cc.SpecializationId })
                .IsUnique();

            modelBuilder.Entity<College>()
                .HasIndex(c => c.InstituteSlug)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.CourseSlug)
                .IsUnique();
            modelBuilder.Entity<Course>()
                .HasIndex(c => c.IsCoaching);

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.IsCompetitiveExam);

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.IsCertification);

            modelBuilder.Entity<CourseCategory>()
                .HasOne(cc => cc.ParentCategory)
                .WithMany(cc => cc.SubCategories)
                .HasForeignKey(cc => cc.ParentCategoryId);

            modelBuilder.Entity<City>()
                .HasIndex(c => c.CitySlug);

            modelBuilder.Entity<Locality>()
                .HasIndex(l => l.LocalitySlug);

            modelBuilder.Entity<Specialization>()
				.HasOne(s => s.Course)
				.WithMany(c => c.Specializations)
				.HasForeignKey(s => s.CourseId);

            modelBuilder.Entity<CollegeCourse>()
                .HasOne(cc => cc.SpecializationNav)
                .WithMany(s => s.CollegeCourses)
                .HasForeignKey(cc => cc.SpecializationId)
                .OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<Specialization>()
				.HasIndex(s => new
					{
						s.CourseId,
						s.SpecializationName
					})
					.IsUnique();

            modelBuilder.Entity<AdminUser>().ToTable("tb_admin_users");

            // STUDY ABROAD CONFIG
            modelBuilder.Entity<Country>()
                .ToTable("tb_country")
                .HasKey(c => c.CountryId);

            modelBuilder.Entity<IntlState>()
                .ToTable("tb_intl_states")
                .HasKey(s => s.StateId);

            modelBuilder.Entity<IntlCity>()
                .ToTable("tb_intl_cities")
                .HasKey(c => c.CityId);

            modelBuilder.Entity<IntlInstitute>()
                .ToTable("tb_intl_institutes")
                .HasKey(i => i.InstituteId);

            modelBuilder.Entity<CourseCategoryIntl>()
                .ToTable("tb_intl_CourseCategory")
                .HasKey(cc => cc.CourseCategoryId);

            modelBuilder.Entity<CourseIntl>()
                .ToTable("tb_intl_Courses")
                .HasKey(c => c.CourseId);

            modelBuilder.Entity<InstituteCourse>()
                .ToTable("tb_intl_InstituteCourse")
                .HasKey(ic => ic.InstituteCourseId);

            // Relationships
            modelBuilder.Entity<Country>()
                .HasMany(c => c.Institutes)
                .WithOne(i => i.Country)
                .HasForeignKey(i => i.CountryId);

            modelBuilder.Entity<IntlCity>()
                .HasMany(ci => ci.Institutes)
                .WithOne(i => i.City)
                .HasForeignKey(i => i.CityId);

            modelBuilder.Entity<InstituteCourse>()
                .HasOne(ic => ic.Institute)
                .WithMany(i => i.InstituteCourses)
                .HasForeignKey(ic => ic.InstituteId);

            modelBuilder.Entity<InstituteCourse>()
                .HasOne(ic => ic.Course)
                .WithMany(c => c.InstituteCourses)
                .HasForeignKey(ic => ic.CourseId);

            modelBuilder.Entity<CourseCategoryIntl>()
                .HasMany(cc => cc.Courses)
                .WithOne(c => c.Category)
                .HasForeignKey(c => c.CourseCategoryId);
            
            modelBuilder.Entity<CourseLevelIntl>()
                .ToTable("tb_intl_course_levels")
                .HasKey(l => l.LevelId);

            modelBuilder.Entity<CourseIntl>()
                .HasOne(c => c.Level)
                .WithMany(l => l.Courses)
                .HasForeignKey(c => c.LevelId);
           
            modelBuilder.Entity<CourseLevelIntl>(entity =>
            {
                entity.ToTable("tb_intl_course_levels");

                entity.HasKey(e => e.LevelId);

                entity.Property(e => e.LevelId)
                    .HasColumnName("level_id");

                entity.Property(e => e.LevelName)
                    .HasColumnName("level_name");

                entity.Property(e => e.LevelSlug)
                    .HasColumnName("level_slug");

                entity.Property(e => e.DisplayOrder)
                    .HasColumnName("display_order");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active");
            });

            // STUDY ABROAD – COURSE ↔ LEVEL
            modelBuilder.Entity<CourseIntl>()
                .HasOne(c => c.Level)
                .WithMany(l => l.Courses)
                .HasForeignKey(c => c.LevelId);

            // STUDY ABROAD – COURSE NAMES LOOKUP
            modelBuilder.Entity<CourseNameIntl>(entity =>
            {
                entity.ToTable("tb_intl_course_names");

                entity.HasKey(e => e.CourseNameId);

                entity.Property(e => e.CourseNameId)
                    .HasColumnName("CourseNameId");

                entity.Property(e => e.CourseName)
                    .HasColumnName("coursename")
                    .IsRequired();

                entity.Property(e => e.DisplayName)
                    .HasColumnName("DisplayName");

                entity.Property(e => e.LevelId)
                    .HasColumnName("LevelId");

                entity.HasOne(e => e.Level)
                .WithMany(l => l.CourseNames)
                .HasForeignKey(e => e.LevelId);
            });

            // STUDY ABROAD – COURSE ↔ COURSE NAME
            modelBuilder.Entity<CourseIntl>(entity =>
            {
                entity.ToTable("tb_intl_Courses");  // ensure table name is set if not already

                entity.Property(e => e.CourseId)
                    .HasColumnName("CourseId");

                entity.Property(e => e.CourseCategoryId)
                    .HasColumnName("CourseCategoryId");

                entity.Property(e => e.CourseName)
                    .HasColumnName("CourseName");

                entity.Property(e => e.LevelId)
                    .HasColumnName("LevelId");

                entity.Property(e => e.DegreeType)
                    .HasColumnName("DegreeType");

                entity.Property(e => e.Slug)
                    .HasColumnName("Slug");

                entity.Property(e => e.IsActive)
                    .HasColumnName("IsActive");

                entity.Property(e => e.CourseNameId)               // NEW column
                    .HasColumnName("CourseNameId");

                entity.HasOne(c => c.CourseNameTemplate)          // NEW relationship
                    .WithMany(n => n.Courses)
                    .HasForeignKey(c => c.CourseNameId);
            });
        }
    }
}
