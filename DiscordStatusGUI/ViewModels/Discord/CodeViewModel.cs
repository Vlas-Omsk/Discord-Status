using DiscordStatusGUI.Extensions;
using DiscordStatusGUI.Libs.DiscordApi;
using DiscordStatusGUI.Locales;
using DiscordStatusGUI.Views;
using DiscordStatusGUI.Views.Discord;
using PinkJson.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DiscordStatusGUI.ViewModels.Discord
{
    class CodeViewModel : TemplateViewModel
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
        string _Header2 = Lang.CodeForm_header2;
        public string Header2
        {
            get => _Header2;
            set
            {
                _Header2 = value;
                OnPropertyChanged("Header2");
            }
        }
        string _Code = "";
        public string Code
        {
            get => _Code;
            set
            {
                _Code = value;
                OnPropertyChanged("Code");
            }
        }
        string _CodeError = "";
        public string CodeError
        {
            get => _CodeError;
            set
            {
                _CodeError = value;
                OnPropertyChanged("CodeError");
            }
        }

        private MFAuthType _DiscordMFAuthType = MFAuthType.Code;
        public Code CodeView;


        public Command LoginCommand { get; private set; }
        public Command SendSMSCommand { get; private set; }
        public Command BackToLoginCommand { get; private set; }

        public CodeViewModel()
        {
            LoginCommand = new Command(Login);
            SendSMSCommand = new Command(SendSMS);
            BackToLoginCommand = new Command(BackToLogin);
        }

        public async void Login()
        {
            LoginButtonEnabled = false;

            await Task.Run(() =>
            {
                CodeView.RestoreCode();
                CodeError = "";

                int code = 0;
                try { code = Convert.ToInt32(Code); }
                catch { CodeError = "Invalid code"; goto end; }

                var auth = Static.Discord.MFAuth(code, _DiscordMFAuthType);
                switch (auth)
                {
                    case MFAuthErrors.InvalidData:
                        var resp = new Json(Static.Discord.LastError);
                        if (resp.IndexByKey("ticket") != -1)
                        {
                            CodeError = (resp["ticket"][0] as JsonObject).Value.ToString();
                            CodeView.WrongOrEmptyCode();
                        }
                        else
                        {
                            CodeError = resp[0].Value.ToString();
                            CodeView.WrongOrEmptyCode();
                        }
                        break;
                    case MFAuthErrors.Error:
                        CodeError = Static.Discord.LastError;
                        break;
                    case MFAuthErrors.Successful:
                        Static.DiscordLoginSuccessful();
                        break;
                }
                end:
                LoginButtonEnabled = true;
            });
        }

        public async void SendSMS()
        {
            LoginButtonEnabled = false;

            await Task.Run(() =>
            {
                CodeView.RestoreCode();
                CodeError = "";

                var auth = Static.Discord.MFAuthSendSMS();
                switch (auth)
                {
                    case MFAuthErrors.InvalidData:
                        var resp = new Json(Static.Discord.LastError);
                        if (resp.IndexByKey("ticket") != -1)
                        {
                            CodeError = (resp["ticket"][0] as JsonObject).Value.ToString();
                            CodeView.WrongOrEmptyCode();
                        }
                        else
                        {
                            CodeError = resp[0].Value.ToString();
                            CodeView.WrongOrEmptyCode();
                        }
                        break;
                    case MFAuthErrors.Error:
                        CodeError = Static.Discord.LastError;
                        break;
                    case MFAuthErrors.Successful:
                        Header2 = "Мы отправили сообщение на " + Static.Discord.Phone + ". Пожалуйста, введите полученный код.";
                        _DiscordMFAuthType = MFAuthType.SMS;
                        break;
                }
                LoginButtonEnabled = true;
            });
        }

        public void BackToLogin()
        {
            Static.CurrentPage = new Login();
        }
    }
}
