using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LookThisWay.Models;
using System.Runtime.CompilerServices;


namespace LookThisWay.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private KinectModel model = new KinectModel();

        public MainViewModel()
        {
            this.model.PropertyChanged += model_PropertyChanged;
        }

        void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public int GameStatus
        {
            get { return (int)this.model.GameStatus; }
        }

        public int FaceRotationX
        {
            get { return this.model.FaceRotationX; }
        }

        public int FaceRotationY
        {
            get { return this.model.FaceRotationY; }
        }

        public int FaceRotationZ
        {
            get { return this.model.FaceRotationZ; }
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartCommand()
        {
            this.model.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
