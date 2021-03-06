﻿using Bogus;
using Bogus.Extensions;
using ConsoleTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


//Top Level C# 

Stopwatch stopwatch = new();
stopwatch.Start();

//await Seed(); //Has some issues with tracking ef core entities 

Console.WriteLine($"Total ms it took=> {stopwatch.ElapsedMilliseconds}");
using DatabaseContext database = new();
stopwatch.Restart();

var courses = await CoursesWithEnrollments(database);
Console.WriteLine($"Total Found : {courses.Count}");
Console.WriteLine($"Total ms it took=> {stopwatch.ElapsedMilliseconds}");


ConsoleTable.From<CourseWithEnrollmentViewModel>(courses)
    .Configure(o => o.NumberAlignment = Alignment.Right).Configure(c => c.EnableCount = true)
    .Write(Format.Alternative);

async Task<List<CourseWithEnrollmentViewModel>> CoursesWithEnrollments(DatabaseContext database)
{
    var q = from c in database.Courses
            join ce in database.CourseEnrollments
            on c.Id equals ce.CourseId
            
            select new CourseWithEnrollmentViewModel(c.Id, c.Title, c.Level, string.Join(",", c.Enrollments.Select(x => $"{x.Student.FirstName} {x.Student.LastName}").ToArray()));
    
    return await q.AsNoTracking().ToListAsync();
    //return await q.AsNoTracking().AsSplitQuery().ToListAsync(); 
    //Above fail with this : There is already an open DataReader associated with this Connection which must be closed - probably caused because EF does not allow concurrent exectuions
}




async Task Seed()
{
    using DatabaseContext database = new();

    Faker<Course> courseFaker = new();
    var courses = courseFaker
        .RuleFor(x => x.Title, value => value.Company.CompanyName())
        .RuleFor(x => x.Level, "Expert")
        .GenerateBetween(10, 20);
    var studentFaker = new Faker<Student>();

    var students = studentFaker
        .RuleFor(x => x.FirstName, v => v.Person.FirstName)
        .RuleFor(x => x.FirstName, v => v.Person.LastName)
        .RuleFor(x => x.SubscriptionLevel, v => v.PickRandom<SubscriptionLevel>())
        .CustomInstantiator(i => new Student(0, i.Person.FirstName, i.Person.LastName, i.PickRandom<SubscriptionLevel>()))
        .GenerateBetween(100, 200);
    database.Courses.AddRange(courses);
    database.Students.AddRange(students);
    await database.SaveChangesAsync(); // To get the IDs

    foreach (var c in courses)
    {
        var random = new Random();
        for (int i = 0; i < random.Next(2, 5); i++)
        {
            var n = random.Next(0, 100);
            var student = students[n];
            if (!database.CourseEnrollments.Any(x => x.CourseId == c.Id && x.StudentId == student.Id))
            {
                database.CourseEnrollments.Add(new CourseEnrollment(c.Id, student.Id, DateTime.UtcNow));
            }
        }
    }

    await database.SaveChangesAsync();
}

record CourseWithEnrollmentViewModel(int id, string title, string level, string studentName);

public class DatabaseContext : DbContext
{
    public static readonly ILoggerFactory DbLoggerFactory
    = LoggerFactory.Create(builder => { builder.AddConsole(); });

    public DbSet<Course> Courses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(DbLoggerFactory)
            .UseSqlServer(@"Data Source=MABOSARAFIQ-L2;Initial Catalog=LearnWithAdnan;Integrated Security=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>().IsMemoryOptimized();
        
        //modelBuilder.Entity<Course>().HasMany<CourseEnrollment>().WithOne(x => x.Course);

        modelBuilder.Entity<Person>().ToTable("Persons").HasDiscriminator<PersonRole>(nameof(Person.Role)).HasValue<Student>(PersonRole.Student).HasValue<Teacher>(PersonRole.Teacher);
        
        //modelBuilder.Entity<Person>().HasMany<CourseEnrollment>().WithOne(x => x.Student);

        modelBuilder.Entity<Person>().IsMemoryOptimized();
        modelBuilder.Entity<Student>().IsMemoryOptimized();
        modelBuilder.Entity<Teacher>().IsMemoryOptimized();

        modelBuilder.Entity<CourseEnrollment>().HasKey(x => new { x.CourseId, x.StudentId });
        
        modelBuilder.Entity<CourseEnrollment>().IsMemoryOptimized();

    }

}


public class Course
{
    public Course() { }
    public Course(int id, string title, string level = null)
    {
        Id = id;
        Title = title;
        Level = level;
    }


    public int Id { get; private set; }

    [MaxLength(50)]
    [Required]
    public string Title { get; private set; }

    [MaxLength(15)]
    [Required]
    public string Level { get; private set; }

    
    public ICollection<CourseEnrollment> Enrollments { get; private set; }
}

public enum PersonRole
{
    Student = 1,
    Teacher = 2
}
public abstract class Person
{
    public Person() { }


    public Person(int id, string firstName, string lastName, PersonRole role)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }

    public int Id { get; private set; }

    [MaxLength(50)]
    [Required]
    public string FirstName { get; private set; }

    [MaxLength(50)]
    [Required]
    public string LastName { get; private set; }

    [Required]
    public PersonRole Role { get; private set; }

}

public enum SubscriptionLevel
{
    Free = 1,
    PaidPerCourse = 2,
    FullAccess = 3

}

public class Teacher : Person
{

    public Teacher() { }
}

public class Student : Person
{
    public Student() { }
    public Student(int id, string firstName, string lastName, SubscriptionLevel subscriptionLevel, PersonRole role = PersonRole.Student) : base(id, firstName, lastName, role)
    {
        SubscriptionLevel = subscriptionLevel;
    }

    [Required]
    public SubscriptionLevel SubscriptionLevel { get; private set; }

    public ICollection<CourseEnrollment> Enrollments { get; private set; }

}

public class CourseEnrollment
{
    public CourseEnrollment() { }


    public CourseEnrollment(int courseId, int studentId, DateTime dateEnrolled)
    {
        CourseId = courseId;
        StudentId = studentId;
        DateEnrolled = dateEnrolled;
    }

    [ForeignKey(nameof(CourseId))]
    [Required]
    public int CourseId { get; private set; }

    public virtual Course Course { get; private set; }

    [ForeignKey(nameof(StudentId))]
    [Required]
    public int StudentId { get; private set; }
    public virtual Student Student { get; private set; }

    [Required]
    public DateTime DateEnrolled { get; private set; }

}

