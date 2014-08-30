using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LookThisWay.Models
{
    class KinectModel : INotifyPropertyChanged
    {

        #region "変数"

        /// <summary>
        /// Kinectセンサーとの接続を示します
        /// </summary>
        private KinectSensor kinect;

        /// <summary>
        /// Kinectセンサーから複数のデータを受け取るためのFrameReaderを示します
        /// </summary>
        private MultiSourceFrameReader reader;

        /// <summary>
        /// 顔情報データの取得元を示します
        /// </summary>
        private FaceFrameSource faceSource;

        /// <summary>
        /// 顔情報データを受け取るためのFrameReaderを示します
        /// </summary>
        private FaceFrameReader faceReader;

        /// <summary>
        /// Kinectセンサーから取得した骨格情報を示します
        /// </summary>
        private Body[] bodies;

        private const FaceFrameFeatures DefaultFaceFrameFeatures = FaceFrameFeatures.PointsInColorSpace
                                        | FaceFrameFeatures.Happy
                                        | FaceFrameFeatures.FaceEngagement
                                        | FaceFrameFeatures.Glasses
                                        | FaceFrameFeatures.LeftEyeClosed
                                        | FaceFrameFeatures.RightEyeClosed
                                        | FaceFrameFeatures.MouthOpen
                                        | FaceFrameFeatures.MouthMoved
                                        | FaceFrameFeatures.LookingAway
                                        | FaceFrameFeatures.RotationOrientation;

        /// <summary>
        /// どちらかの方向をむいたと判定するボーダーの角度を示します
        /// </summary>
        private const int BorderAngle = 30;

        #endregion

        #region "プロパティ"


        private String _Message;

        public String Message
        {
            get { return _Message; }
            set
            {
                this._Message = value;
                OnPropertyChanged();
            }
        }

        private GameStatus _GameStatus;

        public GameStatus GameStatus
        {
            get { return this._GameStatus; }
            set
            {
                this._GameStatus = value;
                OnPropertyChanged();
            }
        }

        private int _FaceRotationX;
        public int FaceRotationX
        {
            get { return this._FaceRotationX; }
            set
            {
                this._FaceRotationX = value;
                OnPropertyChanged();
            }
        }

        private int _FaceRotationY;
        public int FaceRotationY
        {
            get { return this._FaceRotationY; }
            set
            {
                this._FaceRotationY = value;
                OnPropertyChanged();
            }
        }

        private int _FaceRotationZ;
        public int FaceRotationZ
        {
            get { return this._FaceRotationZ; }
            set
            {
                this._FaceRotationZ = value;
                OnPropertyChanged();
            }
        }



        #endregion

        /// <summary>
        /// センサーからの情報の取得を開始します
        /// </summary>
        public void Start()
        {
            Initialize();
        }

        /// <summary>
        /// センサーからの情報の取得を終了します
        /// </summary>
        public void Stop()
        {
            if (this.reader != null)
                this.reader.Dispose();

            if (this.faceSource != null)
                this.faceSource.Dispose();

            if (this.faceReader != null)
                this.faceReader.Dispose();

            this.kinect.Close();
            this.kinect = null;
        }

        /// <summary>
        /// Kinectセンサーを初期化し、データの取得用に各種変数を初期化します
        /// </summary>
        private void Initialize()
        {
            // Kinectセンサーを取得
            this.kinect = KinectSensor.GetDefault();

            if (this.kinect == null) return;

            // KinectセンサーからBody(骨格情報)とColor(色情報)を取得するFrameReaderを作成
            this.reader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
            this.reader.MultiSourceFrameArrived += OnMultiSourceFrameArrived;

            // FaceFrameSourceを作成
            faceSource = new FaceFrameSource(kinect, 0, DefaultFaceFrameFeatures);

            // Readerを作成する
            faceReader = faceSource.OpenReader();

            // FaceReaderからフレームを受け取ることができるようになった際に発生するイベント
            faceReader.FrameArrived += OnFaceFrameArrived;
            // FaceFrameSourceが指定されたTrackingIdのトラッキングに失敗した際に発生するイベント
            faceSource.TrackingIdLost += OnTrackingIdLost;

            // センサーの開始
            kinect.Open();
        }

        private void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            this.faceSource.TrackingId = 0;
        }

        private void OnFaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (var faceFrame = e.FrameReference.AcquireFrame())
            {
                if (faceFrame == null) return;

                var result = faceFrame.FaceFrameResult;

                if (result == null)
                    return;

                // 顔の回転に関する結果を取得する
                var rotation = result.FaceRotationQuaternion;

                // 回転角度を取得する
                int x, y, z;
                ExtractFaceRotationInDegrees(rotation,out x,out y,out z);
                this.FaceRotationX = x;
                this.FaceRotationY = y;
                this.FaceRotationZ = z;

                //  値によってStatusを変更する
                if (Math.Abs(x) > Math.Abs(y))
                {
                    // x方向(縦方向のほうが大きい) -> 上下のどちらかかもしれない
                    if (x > BorderAngle)
                    {
                        this.GameStatus = Models.GameStatus.Up;
                    }
                    else if(x < - BorderAngle)
                    {
                        this.GameStatus = Models.GameStatus.Down;
                    }
                    else
                    {
                        this.GameStatus = Models.GameStatus.WaitingForAction;
                    }
                }
                else
                {
                    if (y > BorderAngle)
                    {
                        this.GameStatus = Models.GameStatus.Left;
                    }
                    else if (y < -BorderAngle)
                    {
                        this.GameStatus = Models.GameStatus.Right;
                    }
                    else
                    {
                        this.GameStatus = Models.GameStatus.WaitingForAction;
                    }
                }
                 
            }
        }

        private void OnMultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();
            if (frame == null) return;

            // BodyFrameに関してフレームを取得する
            using (var bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                        bodies = new Body[bodyFrame.BodyCount];

                    // 骨格データを格納
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    // FaceTrackingが開始されていないか確認
                    if (!this.faceSource.IsTrackingIdValid)
                    {
                        // トラッキング先の骨格を選択
                        var target = (from body in this.bodies where body.IsTracked select body).FirstOrDefault();
                        if (target != null)
                            // 検出されたBodyに対してFaceTrackingを行うよう、FaceFrameSourceを設定
                            this.faceSource.TrackingId = target.TrackingId;
                    }
                }
            }
        }

        private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
        {
            double x = rotQuaternion.X;
            double y = rotQuaternion.Y;
            double z = rotQuaternion.Z;
            double w = rotQuaternion.W;

            double yawD, pitchD, rollD;
            pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
            yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
            rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

            double increment = 5;
            pitch = (int)((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * (int)increment;
            yaw = (int)((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * (int)increment;
            roll = (int)((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * (int)increment;
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}
