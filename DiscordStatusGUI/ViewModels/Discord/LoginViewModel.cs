using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Views;
using DiscordStatusGUI.Views.Discord;
using PinkJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DiscordStatusGUI.ViewModels.Discord
{
    class LoginViewModel : TemplateViewModel
    {
        bool _LoginButtonEnabled = true;
        public bool LoginButtonEnabled
        {
            get => _LoginButtonEnabled;
            set
            {
                _LoginButtonEnabled = value;
                OnPropertyChanged("LoginButtonEnabled");
            }
        }
        public string _Email = "";
        public string Email
        {
            get => _Email;
            set
            {
                _Email = value;
                OnPropertyChanged("Email");
            }
        }
        public string _Password = "";
        public string Password
        {
            get => _Password;
            set
            {
                _Password = value;
                OnPropertyChanged("Password");
            }
        }
        string _EmailError;
        public string EmailError
        {
            get => _EmailError;
            set
            {
                _EmailError = value;
                OnPropertyChanged("EmailError");
            }
        }
        string _PasswordError;
        public string PasswordError
        {
            get => _PasswordError;
            set
            {
                _PasswordError = value;
                OnPropertyChanged("PasswordError");
            }
        }

        public Login LoginView;


        public Command LoginCommand { get; private set; }
        public Command ForgotPasswordCommand { get; private set; }
        public Command SkipLoginCommand { get; private set; }

        public LoginViewModel()
        {
            LoginCommand = new Command(Login);
            ForgotPasswordCommand = new Command(ForgotPassword);
            SkipLoginCommand = new Command(SkipLogin);
        }


        public async void Login()
        {
            LoginButtonEnabled = false;
            Static.Discord.Email = Email;
            Static.Discord.Password = Password;

            await Task.Run(() =>
            {
                LoginView.RestoreEmail();
                LoginView.RestorePassword();
                EmailError = PasswordError = "";

                var auth = Static.Discord.Auth();
                switch (auth)
                {
                    case Libs.DiscordApi.AuthErrors.LoginError:
                        var resp = new Json(Static.Discord.LastError);
                        if (resp.IndexByKey("email") != -1)
                        {
                            EmailError = resp["email"][0].ToString();
                            LoginView.WrongOrEmptyEmail();
                        }
                        if (resp.IndexByKey("password") != -1)
                        {
                            PasswordError = resp["password"][0].ToString();
                            LoginView.WrongOrEmptyPassword();
                        }
                        break;
                    case Libs.DiscordApi.AuthErrors.Error:
                        EmailError = Static.Discord.LastError;
                        break;
                    case Libs.DiscordApi.AuthErrors.MultiFactorAuthentication:
                        Static.MainWindow.Dispatcher.Invoke(() => 
                            Static.CurrentPage = new Code());
                        break;
                    case Libs.DiscordApi.AuthErrors.Successful:
                        Static.MainWindow.Dispatcher.Invoke(() =>
                            Static.DiscordLoginSuccessful());
                        break;
                }
                LoginButtonEnabled = true;
            });
        }

        public async void ForgotPassword()
        {
            Static.Discord.Email = Email;

            await Task.Run(() =>
            {
                LoginView.RestoreEmail();
                LoginView.RestorePassword();
                EmailError = PasswordError = "";

                var forgot = Static.Discord.ForgotPassword();
                switch (forgot)
                {
                    case Libs.DiscordApi.ForgotPasswordErrors.DataError:
                        var resp = new Json(Static.Discord.LastError);
                        if (resp.IndexByKey("email") != -1)
                        {
                            EmailError = resp["email"][0].ToString();
                            LoginView.WrongOrEmptyEmail();
                        }
                        break;
                    case Libs.DiscordApi.ForgotPasswordErrors.Error:
                        EmailError = Static.Discord.LastError;
                        break;
                    case Libs.DiscordApi.ForgotPasswordErrors.Successful:
                        Static.Dialogs.MessageBoxShow("Инструкции отправлены", "Мы отправили инструкции по смене пароля на " + Static.Discord.Email + ", пожалуйста, проверьте папки «Входящие» и «Спам».", new System.Collections.ObjectModel.ObservableCollection<Models.ButtonItem>() { Static.Dialogs.ButtonOk });
                        break;
                }
            });
        }

        public void SkipLogin()
        {
            Static.MainWindow.ReplaceWithWaves(new VerticalTabControl());
        }
    }
}
