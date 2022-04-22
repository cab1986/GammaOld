﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Gamma.Common;
using Gamma.Entities;
using System.Windows;

namespace Gamma.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class UserEditViewModel : SaveImplementedViewModel
    {
        /// <summary>
        /// Initializes a new instance of the NewUserViewModel class.
        /// </summary>
        public UserEditViewModel()
        {
            _isNewUser = true;
            User = new Users { UserID = SqlGuidUtil.NewSequentialid() };
            ChangePassEnabled = false;
            InitializeFields();
        }

        public UserEditViewModel(Guid userid)
        {
            ChangePassEnabled = true;
            UserID = userid;
            User = GammaBase.Users.Include(u => u.Places).First(u => u.UserID == userid);
            Login = User.Login;
            Name = User.Name;
            UserPlaces = new ObservableCollection<PlaceID>(User.Places.Select(p => new PlaceID()
            {
                Value = p.PlaceID
            }));
            Post = User.Post;
            RoleID = User.RoleID;
            DepartmentID = User.DepartmentID;
            IsDBAdmin = User.DBAdmin;
            ShiftID = User.ShiftID;
            PrimePlaceID = User.PrimePlaceID;
            InitializeFields();
        }
        private void InitializeFields()
        {
            ChangePasswordCommand = new DelegateCommand(ChangePassword);
            AddPlaceCommand = new DelegateCommand(() => UserPlaces.Add(Places.Where(p => !UserPlaces.Select(up => up.Value).Contains(p.PlaceID))
                .Select(p => new PlaceID() { Value = p.PlaceID }).FirstOrDefault()), 
                () => Places.Any(p => !UserPlaces.Select(up => up.Value).Contains(p.PlaceID)));
            DeletePlaceCommand = new DelegateCommand(() => UserPlaces.Remove(SelectedPlaceID), () => SelectedPlaceID != null);
            Places = new ObservableCollection<Places>(WorkSession.Places);
            Roles = new ObservableCollection<Roles>(GammaBase.Roles);
            Departments = new ObservableCollection<Departments>(GammaBase.Departments);
        }
        public DelegateCommand AddPlaceCommand { get; private set; }
        public DelegateCommand DeletePlaceCommand { get; private set; }
        public PlaceID SelectedPlaceID { get; set; }
        private readonly bool _isNewUser;
        public bool IsDBAdmin { get; set; }
        [Required(ErrorMessage=@"Поле логин не может быть пустым")]
        public string Login
        {
            get
            {
                return _login;
            }
            set
            {
            	_login = value;
                RaisePropertyChanged("Login");
            }
        }
        [Required(ErrorMessage=@"Поле ФИО не может быть пустым")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
            	_name = value;
                RaisePropertyChanged("Name");
            }
        }
        [Required(ErrorMessage=@"Пароль не может быть пустым")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string Password { get; set; }
        public string Post { get; set; }
        [Required(ErrorMessage = @"Укажите роль доступа пользователя")]
        public Guid RoleID { get; set; }
        public byte ShiftID { get; set; }

        [Required(ErrorMessage = @"Укажите службу пользователя")]
        public short? DepartmentID { get; set; }

        [Required(ErrorMessage = @"Основной передел пользователя")]
        public int? _primePlaceID { get; set; }
        public int? PrimePlaceID
        {
            get
            {
                return _primePlaceID;
            }
            set
            {
                if (value != null && UserPlaces.FirstOrDefault(p=> p.Value == value) == null)
                    MessageBox.Show("Такого подразделения нет в списке подразделений", "Ошибка", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                else
                    _primePlaceID = value;
                RaisePropertyChanged("PrimePlaceID");
            }
        }

        private Guid UserID { get; set; }
        private string _login;
        private string _name;
        private Users User { get; set; }
        

        public override bool SaveToModel()
        {
            
            User.Login = Login;
            User.Name = Name;
            User.Places.Clear();
            foreach (var placeId in UserPlaces)
            {
                User.Places.Add(GammaBase.Places.Find(placeId.Value));
            }
            User.RoleID = RoleID;
            User.DepartmentID = DepartmentID;
            User.ShiftID = ShiftID;
            User.Post = Post;
            User.DBAdmin = IsDBAdmin;
            User.PrimePlaceID = PrimePlaceID;
            if (_isNewUser)
            {
                GammaBase.Users.Add(User);
            }
            GammaBase.SaveChanges();
            if (_isNewUser)
                DB.RecreateUserInDb(User.UserID, Password, GammaBase);
            return true;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DelegateCommand ChangePasswordCommand { get; private set; }
        private void ChangePassword()
        {
            DB.ChangeUserPassword(User.UserID, Password, GammaBase);
        }
        public bool ChangePassEnabled { get; set; }
        [RequiredCollection(ErrorMessage = @"Необходимо выбрать подразделение")]
        public ObservableCollection<PlaceID> UserPlaces { get; set; } = new ObservableCollection<PlaceID>();
        public ObservableCollection<Places> Places { get; set; }
        public ObservableCollection<Roles> Roles { get; set; }
        public ObservableCollection<Departments> Departments { get; set; }

        public class PlaceID
        {
            public int Value
            {
                get;
                set;
            }
        }
    }
}