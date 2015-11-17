using System;
using System.Linq;
using Microsoft.Kinect;
using egmtest;


namespace EgmSmallTest
{
    class KinectProgram
    {
        KinectSensor kinect = null;
        Skeleton[] skeletonData;
        Sensor abbSensor = null;
        public int limitsKinectHeight = 750; //To avoid collisions, robots z direction
        public int limitsKinectSides = 650;  //To avoid collisions, robots xy direction

        public KinectProgram(Sensor abbSensor)
        {
            this.abbSensor = abbSensor;
            this.abbSensor.Start();
            StartKinectST();
        }

        void StartKinectST()
        {
            kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected); // Get first Kinect Sensor
            kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default; // Enable skeletal tracking, default = standing

            TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
            {
                smoothingParam.Smoothing = 0.5f;
                smoothingParam.Correction = 0.5f;
                smoothingParam.Prediction = 0.5f;
                smoothingParam.JitterRadius = 0.05f;
                smoothingParam.MaxDeviationRadius = 0.04f;
            };

            kinect.SkeletonStream.Enable(smoothingParam);
            skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady); // Get Ready for Skeleton Ready Events

            kinect.Start(); // Start Kinect sensor
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {   
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);// get the skeletal information in this frame
                    TrackClosestSkeleton();
                    SetCoordinates(skeletonData[0]);                    
                }
            }
        }


        private void TrackClosestSkeleton()
        {
            if (this.kinect != null && this.kinect.SkeletonStream != null)
            {
                if (!this.kinect.SkeletonStream.AppChoosesSkeletons)
                {
                    this.kinect.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
                }

                float closestDistance = 10000f; // Start with a far enough distance
                int closestID = 0;

                foreach (Skeleton skeleton in this.skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                {
                    if (skeleton.Position.Z < closestDistance)
                    {
                        closestID = skeleton.TrackingId;
                        closestDistance = skeleton.Position.Z;
                        skeletonData[0] = skeleton;
                    }
                }

                if (closestID > 0)
                {
                    this.kinect.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton
                }
            }
        }

        public void SetCoordinates(Skeleton skeleton)
        {
            var center = skeleton.Joints[JointType.HipCenter];
            var centerX = center.Position.X;
            var centerY = center.Position.Y;

            var handRight = skeleton.Joints[JointType.HandRight];
            var handRightX = handRight.Position.X;
            var handRightY = handRight.Position.Y;

            var X = 1000*(centerX - handRightX);
            var Y = 1000*(centerY - handRightY);

            X = Convert.ToInt32(X);
            Y = Convert.ToInt32(Y);

            if (X > limitsKinectSides)
            {
                X = limitsKinectSides;
            }

            else if (X < -limitsKinectSides)
            {
                X = -limitsKinectSides; 
            }

            if (Y > limitsKinectHeight)
            {
                Y = limitsKinectHeight;
            }

            else if(Y < -limitsKinectHeight)
            {
                Y = -limitsKinectHeight;
            }
            

            abbSensor.X = X;
            abbSensor.Y = Y;

        }
    }
}
