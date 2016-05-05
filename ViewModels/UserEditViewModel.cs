using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Models;
using DevExpress.Mvvm;
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
        public UserEditViewModel(GammaEntities gammaBase = null): base(gammaBase)
        {
            _isNewUser = true;
            User = new Users() { UserID = SqlGuidUtil.NewSequentialid() };
            ChangePassEnabled = false;
            InitializeFields();
        }
        public UserEditViewModel(Guid userid, GammaEntities gammaBase = null): base(gammaBase)
        {
            ChangePassEnabled = true;
            UserID = userid;
            User = GammaBase.Users.Include(u => u.Places).FirstOrDefault(u => u.UserID == userid);
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
            ChangePasswordCommand = new DelegateCommand(ChangePassword);
            Places = new ObservableCollection<Places>(GammaBase.Places);
            Roles = new ObservableCollection<Roles>(GammaBase.Roles);
        }
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
        public string Password { get; set; }
        [Required(ErrorMessage=@"Не указано подразделение")]
        public int PlaceID { get; set; }
        public string Post { get; set; }
        [Required(ErrorMessage = @"Укажите роль доступа пользователя")]
        public Guid RoleID { get; set; }
        public byte ShiftID { get; set; }

        private Guid UserID { get; set; }
        private string _login;
        private string _name;
        private Users User { get; set; }

        protected override void SaveToModel(GammaEntities gammaBase = null)
        {
            gammaBase = gammaBase ?? DB.GammaDb;
            base.SaveToModel(gammaBase);
            User.Login = Login;
            User.Name = Name;
            User.Places.Clear();
            User.Places.Add(GammaBase.Places.Find(PlaceID));
            User.RoleID = RoleID;
            User.ShiftID = ShiftID;
            User.Post = Post;
            User.DBAdmin = IsDBAdmin;
            if (_isNewUser)
            {
                GammaBase.Users.Add(User);
            }
            GammaBase.SaveChanges();
            if (_isNewUser)
                DB.RecreateUserInDb(User.UserID, Password);
        }
        public DelegateCommand ChangePasswordCommand { get; private set; }
        private void ChangePassword()
        {
            DB.ChangeUserPassword(User.UserID, Password);
        }
        public bool ChangePassEnabled { get; set; }
        public ObservableCollection<Places> Places { get; set; }
        public ObservableCollection<Roles> Roles { get; set; }
    }
}