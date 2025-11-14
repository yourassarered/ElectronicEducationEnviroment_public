using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace ElectronicEducationEnviroment.Admin.Pages.CourcePage
{
    public partial class AssignTeachersWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly Cours _course;

        public AssignTeachersWindow(Cours course)
        {
            InitializeComponent();
            _context = App.Context;
            _course = course;
            LoadLists();
        }

        private void LoadLists()
        {
            var assignedTeacherIds = _context.CourseTeachers
                .Where(ct => ct.IdCourse == _course.IdCourse)
                .Select(ct => ct.IdTeacher)
                .ToList();

            var teachers = _context.Users
                .Where(u => u.IdRole == 2)
                .Select(u => new TeacherViewModel
                {
                    IdUser = u.IdUser,
                    FullName = u.LastName + " " + u.FirstName
                })
                .ToList();

            AssignedTeachersList.ItemsSource = teachers
                .Where(t => assignedTeacherIds.Contains(t.IdUser))
                .ToList();

            AvailableTeachersList.ItemsSource = teachers
                .Where(t => !assignedTeacherIds.Contains(t.IdUser))
                .ToList();
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            var selected = AvailableTeachersList.SelectedItems.Cast<TeacherViewModel>().ToList();

            foreach (var teacher in selected)
            {
                bool exists = _context.CourseTeachers
                    .Any(ct => ct.IdCourse == _course.IdCourse && ct.IdTeacher == teacher.IdUser);

                if (!exists)
                {
                    _context.CourseTeachers.Add(new CourseTeacher
                    {
                        IdCourse = _course.IdCourse,
                        IdTeacher = teacher.IdUser
                    });
                }
            }

            _context.SaveChanges();
            LoadLists();
        }

        private void Unassign_Click(object sender, RoutedEventArgs e)
        {
            var selected = AssignedTeachersList.SelectedItems.Cast<TeacherViewModel>().ToList();

            foreach (var teacher in selected)
            {
                var link = _context.CourseTeachers
                    .FirstOrDefault(ct => ct.IdCourse == _course.IdCourse && ct.IdTeacher == teacher.IdUser);

                if (link != null)
                    _context.CourseTeachers.Remove(link);
            }

            _context.SaveChanges();
            LoadLists();
        }
    }

    public class TeacherViewModel
    {
        public int IdUser { get; set; }
        public string FullName { get; set; }
    }
}
