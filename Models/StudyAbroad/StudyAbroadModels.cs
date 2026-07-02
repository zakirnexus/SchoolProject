using System;
using System.Collections.Generic;

namespace SchoolProject.Models.StudyAbroad
{
    public class Country
    {
        public int CountryId { get; set; }
        public string? CountryCode { get; set; }
        public string CountryName { get; set; }
        public string? Slug { get; set; }
        public bool IsActive { get; set; }

        public ICollection<IntlState> States { get; set; }
        public ICollection<IntlInstitute> Institutes { get; set; }
    }

    public class IntlState
    {
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string StateName { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }

        public Country Country { get; set; }
        public ICollection<IntlCity> Cities { get; set; }
    }

    public class IntlCity
    {
        public int CityId { get; set; }
        public int? StateId { get; set; }
        public string CityName { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }

        public IntlState State { get; set; }
        public ICollection<IntlInstitute> Institutes { get; set; }
    }

    public class IntlInstitute
    {
        public int InstituteId { get; set; }
        public int? LegacyInstituteId { get; set; }
        public int CountryId { get; set; }
        public int? CityId { get; set; }

        public string Slug { get; set; }
        public string InstituteName { get; set; }
        public string? InstituteNameAlt { get; set; }
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string? Locality { get; set; }

        public string? UniversityName { get; set; }
        public string? SyllabusAffiliation { get; set; }
        public string? InstituteStatus { get; set; }
        public string? InstituteStatusSub { get; set; }
        public string? InstituteLevel { get; set; }

        public string? Telephone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Email2 { get; set; }

        public string? Accreditation { get; set; }
        public string? ApprovedBy { get; set; }
        public string? InstituteRanking { get; set; }
        public int? IndiaTodayRanking { get; set; }

        public string? WebsiteUrl { get; set; }
        public string? DescriptionHtml { get; set; }
        public string? CoEd { get; set; }
        public string? Keywords { get; set; }
        public string? LogoPath { get; set; }
        public bool? IsPaidListing { get; set; }
        public string? Photos { get; set; }
        public int? EstablishedYear { get; set; }
        public string? MetaDescription { get; set; }
        public string? Consultants { get; set; }
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Country Country { get; set; }
        public IntlCity? City { get; set; }
        public ICollection<InstituteCourse>? InstituteCourses { get; set; }
    }

    public class CourseCategoryIntl
    {
        public int CourseCategoryId { get; set; }
        public string Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public ICollection<CourseIntl> Courses { get; set; }
    }

    public class CourseIntl
    {
        public int CourseId { get; set; }
        public int? CourseCategoryId { get; set; }
        public int? CourseNameId { get; set; }   // FK to tb_intl_course_names
        public string CourseName { get; set; }   // full text, or you can split this later
        public int? LevelId { get; set; }
        public string? DegreeType { get; set; }
        public string? Slug { get; set; }
        public bool IsActive { get; set; }

        public CourseCategoryIntl? Category { get; set; }
        public CourseLevelIntl? Level { get; set; }
        public CourseNameIntl? CourseNameTemplate { get; set; } // navigation
        public ICollection<InstituteCourse>? InstituteCourses { get; set; }
    }

    public class CourseNameIntl
{
    public int CourseNameId { get; set; }
    public string CourseName { get; set; }       // e.g. BEng, BSc, MBA
    public string? DisplayName { get; set; }     // e.g. Bachelor of Engineering
    public int? LevelId { get; set; }       // UG / PG / etc.
    public CourseLevelIntl? Level { get; set; }

    public ICollection<CourseIntl>? Courses { get; set; }
}
    public class InstituteCourse
    {
        public int InstituteCourseId { get; set; }
        public int InstituteId { get; set; }
        public int CourseId { get; set; }

        public decimal? DurationYears { get; set; }
        public string DurationText { get; set; }

        public decimal? CostMin { get; set; }
        public decimal? CostMax { get; set; }
        public string CostPer { get; set; }
        public string CostCurrency { get; set; }
        public bool? IsApproximateCost { get; set; }

        public string RawHtml { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IntlInstitute Institute { get; set; }
        public CourseIntl Course { get; set; }
    }

   

    public class CourseLevelIntl
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string? LevelSlug { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? IsActive { get; set; }

        public ICollection<CourseIntl>? Courses { get; set; }
        public ICollection<CourseNameIntl>? CourseNames { get; set; } // NEW
        
    }
}
