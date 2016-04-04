using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Models;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Data.Entity;


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
            IsNewUser = true;
            User = new Users() { UserID = SQLGuidUtil.NewSequentialId() };
            ChangePassEnabled = false;
            InitializeFields();
        }
        public UserEditViewModel(Guid userID)
        {
            ChangePassEnabled = true;
            UserID = userID;
            User = DB.GammaBase.Users.Include(u => u.Places).Where(u => u.UserID == userID).FirstOrDefault();
            Login = User.Login;
            Name = User.Name;
            PlaceID = User.Places.First().PlaceID;
            Post = User.Post;
            RoleID = User.RoleID;
            IsDBAdmin = User.DBAdmin;
            ShiftID = User.ShiftID;
            InitializeFields();
        }
        private void InitializeFields()
        {
            ChangePasswordCommand = new RelayCommand(ChangePassword);
            Places = new ObservableCollection<Places>(DB.GammaBase.Places.Select(p => p));
            Roles = new ObservableCollection<Roles>(DB.GammaBase.Roles.Select(r => r));
        }
        private bool IsNewUser = false;
        public bool IsDBAdmin { get; set; }
        [Required(ErrorMessage="Поле логин не может быть пустым")]
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
        [Required(ErrorMessage="Поле ФИО не может быть пустым")]
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
        [Required(ErrorMessage="Пароль не может быть пустым")]
        public string Password { get; set; }
        [Required(ErrorMessage="Не указано подразделение")]
        public int PlaceID { get; set; }
        public string Post { get; set; }
        [Required(ErrorMessage = "Укажите роль доступа пользователя")]
        public Guid RoleID { get; set; }
        public byte ShiftID
        {
            get
            {
                return _shiftID;
            }
            set
            {
                _shiftID = value;
            }
        }
        private Guid UserID { get; set; }
        private string _login;
        private string _name;
        private byte _shiftID = 0;
        private Users User { get; set; }
        
        public override void SaveToModel()
        {
            base.SaveToModel();
            User.Login = Login;
            User.Name = Name;
            User.Places.Clear();
            User.Places.Add(DB.GammaBase.Places.Find(PlaceID));
            User.RoleID = RoleID;
            User.ShiftID = ShiftID;
            User.Post = Post;
            User.DBAdmin = IsDBAdmin;
            if (IsNewUser)
            {
                DB.GammaBase.Users.Add(User);
            }
            DB.GammaBase.SaveChanges();
            if (IsNewUser)
                DB.RecreateUserInDB(User.UserID, Password);
        }
        public RelayCommand ChangePasswordCommand { get; private set; }
        private void ChangePassword()
        {
            DB.ChangeUserPassword(User.UserID, Password);
        }
        public bool ChangePassEnabled { get; set; }
        public ObservableCollection<Places> Places { get; set; }
        public ObservableCollection<Roles> Roles { get; set; }
    }
}