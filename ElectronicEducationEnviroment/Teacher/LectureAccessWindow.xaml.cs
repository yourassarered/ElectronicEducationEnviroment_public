using ElectronicEducationEnviroment.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElectronicEducationEnviroment.Teacher
{
    /// <summary>
    /// Логика взаимодействия для LectureAccessWindow.xaml
    /// </summary>
    public partial class LectureAccessWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly int _lectureId;

        public LectureAccessWindow(int lectureId)
        {
            InitializeComponent();
            _context = App.Context;
            _lectureId = lectureId;
            LoadData();
        }

        private void LoadData()
        {
            var allGroups = _context.Groups.ToList();

            var accessIds = _context.LectureAccesses
                .Where(a => a.IdLecture == _lectureId)
                .Select(a => a.IdGroup)
                .ToList();

            var groupsWithAccess = allGroups
                .Where(g => accessIds.Contains(g.IdGroup))
                .Select(g => new GroupViewModel { IdGroup = g.IdGroup, GroupName = g.GroupCode })
                .ToList();

            var groupsWithoutAccess = allGroups
                .Where(g => !accessIds.Contains(g.IdGroup))
                .Select(g => new GroupViewModel { IdGroup = g.IdGroup, GroupName = g.GroupCode })
                .ToList();

            HasAccessList.ItemsSource = groupsWithAccess;
            NoAccessList.ItemsSource = groupsWithoutAccess;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var selected = NoAccessList.SelectedItems.Cast<GroupViewModel>().ToList();
            if (selected.Count == 0) return;

            foreach (var group in selected)
            {
                _context.LectureAccesses.Add(new LectureAccess
                {
                    IdLecture = _lectureId,
                    IdGroup = group.IdGroup
                });
            }

            _context.SaveChanges();
            LoadData();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var selected = HasAccessList.SelectedItems.Cast<GroupViewModel>().ToList();
            if (selected.Count == 0) return;

            foreach (var group in selected)
            {
                var record = _context.LectureAccesses
                    .FirstOrDefault(a => a.IdLecture == _lectureId && a.IdGroup == group.IdGroup);

                if (record != null)
                    _context.LectureAccesses.Remove(record);
            }

            _context.SaveChanges();
            LoadData();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public class GroupViewModel
        {
            public int IdGroup { get; set; }
            public string GroupName { get; set; }
        }
    }
}
