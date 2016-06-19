using System;
using System.Linq;
using Microsoft.Kinect;


namespace EgmSmallTest
{
    class KinectProgram
    {
        KinectSensor _kinect = null;
        Skeleton[] _skeletonData;
        Sensor abbSensor = null;
        public int LimitsKinectHeight = 750; //To avoid collisions, robots z direction
        public int LimitsKinectSides = 650;  //To avoid collisions, robots xy direction

        public KinectProgram(Sensor abbSensor)
        {
            this.abbSensor = abbSensor;
            this.abbSensor.Start();
            StartKinectSt();
        }

        void StartKinectSt()
        {
            // Get first Kinect Sensor
            _kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);
            // Enable skeletal tracking, default = standing
            _kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default; 

            // Define smoothing parameters
            TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
            {
                smoothingParam.Smoothing = 0.5f;
                smoothingParam.Correction = 0.5f;
                smoothingParam.Prediction = 0.5f;
                smoothingParam.JitterRadius = 0.05f;
                smoothingParam.MaxDeviationRadius = 0.04f;
            }

            // Smooth the position data
            _kinect.SkeletonStream.Enable(smoothingParam);
            // Allocate ST data
            _skeletonData = new Skeleton[_kinect.SkeletonStream.FrameSkeletonArrayLength];
            // Get Ready for Skeleton Ready Events
            _kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);
            // Start Kinect sensor
            _kinect.Start(); 
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {   
            // Open the Skeleton frame
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) 
            {
                // check that a frame is available
                if (skeletonFrame != null && this._skeletonData != null) 
                {
                    // get the skeletal information in this frame
                    skeletonFrame.CopySkeletonDataTo(this._skeletonData);
                    TrackClosestSkeleton();
                    SetCoordinates(_skeletonData[0]);                    
                }
            }
        }


        private void TrackClosestSkeleton()
        {
            if (this._kinect != null && this._kinect.SkeletonStream != null)
            {
                if (!this._kinect.SkeletonStream.AppChoosesSkeletons)
                {
                    // Ensure AppChoosesSkeletons is set
                    this._kinect.SkeletonStream.AppChoosesSkeletons = true; 
                }

                // Start with a far enough distance
                float closestDistance = 10000f; 
                int closestId = 0;

                foreach (Skeleton skeleton in this._skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                {
                    if (skeleton.Position.Z < closestDistance)
                    {
                        closestId = skeleton.TrackingId;
                        closestDistance = skeleton.Position.Z;
                        _skeletonData[0] = skeleton;
                    }
                }

                if (closestId > 0)
                {
                    // Track this skeleton
                    this._kinect.SkeletonStream.ChooseSkeletons(closestId); 
                }
            }
        }

        // Registrering the right hand position in relation the center of the hip
        public void SetCoordinates(Skeleton skeleton)
        {
            var center = skeleton.Joints[JointType.HipCenter];
            var centerX = center.Position.X;
            var centerY = center.Position.Y;

            var handRight = skeleton.Joints[JointType.HandRight];
            var handRightX = handRight.Position.X;
            var handRightY = handRight.Position.Y;

            // Multiplying with 1000 to get the position in mm
            var x = 1000*(centerX - handRightX);
            var y = 1000*(centerY - handRightY);

            x = Convert.ToInt32(x);
            y = Convert.ToInt32(y);

            // Set limits to avoid the robot from colliding with its surroundings
            if (x > LimitsKinectSides)
            {
                x = LimitsKinectSides;
            }

            else if (x < -LimitsKinectSides)
            {
                x = -LimitsKinectSides; 
            }

            if (y > LimitsKinectHeight)
            {
                y = LimitsKinectHeight;
            }

            else if(y < -LimitsKinectHeight)
            {
                y = -LimitsKinectHeight;
            }
            
            // Send the position data to the Sensor class
            abbSensor.X = x;
            abbSensor.Z = y;
        }
    }
}
