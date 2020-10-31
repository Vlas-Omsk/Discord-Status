using DiscordStatusGUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.ViewModels.Dialogs
{
    class SaveChangesBoxViewModel : TemplateViewModel
    {
        private Command _CancelCommand;
        public Command CancelCommand
        {
            get => _CancelCommand;
            set
            {
                _CancelCommand = value;
                OnPropertyChanged("CancelCommand");
            }
        }
        private Command _ApplyCommand;
        public Command ApplyCommand
        {
            get => _ApplyCommand;
            set
            {
                _ApplyCommand = value;
                OnPropertyChanged("ApplyCommand");
            }
        }


        public SaveChangesBoxViewModel()
        {
        }
    }
}
