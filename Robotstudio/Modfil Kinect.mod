MODULE EGM_test_UDP
    
    VAR egmident egmID1;
    VAR egmstate egmSt1;
    
    CONST egm_minmax egm_minmax_lin1:=[-1,1]; !in mm
    CONST egm_minmax egm_minmax_rot1:=[-2,2];! in degees
    
    CONST robtarget pcal:=[[447,-263,272],[0.01697,-0.50171,0.86445,-0.02678],[0,0,-1,0],[9E9,9E9,9E9,9E9,9E9,9E9]];

    PERS tooldata UISpenholder:=[TRUE,[[0,0,114.25],[1,0,0,0]],[1,[-0.095984607,0.082520613,38.69176324],[1,0,0,0],0,0,0]];
    
    TASK PERS wobjdata Kinect:=[FALSE,TRUE,"",[[0,0,0],[1,0,0,0]],[[480,-300,200],[1,0,0,0]]];
    
    VAR pose posecorTable:=[[447,-263,272],[0.01697,-0.50171,0.86445,-0.02678]];
    VAR pose posesenTable:=[[447,-263,272],[0.01697,-0.50171,0.86445,-0.02678]];
  

    PROC main()
        ! Move to start position. Fine point is demanded.
        MoveJ pcal,v3000,fine,UISpenholder;
        testuc_UDP;   
    ENDPROC
  
    
    PROC testuc_UDP()
        EGMReset egmID1;
        EGMGetId egmID1;
        egmSt1 := EGMGetState(egmID1);
        TPWrite "EGM state: "\Num := egmSt1;
        
        IF egmSt1 <= EGM_STATE_CONNECTED THEN
            ! Set up the EGM data source: UdpUc server using device "EGMsensor:"and configuration "default"
            EGMSetupUC ROB_1, egmID1, "default", "EGMSensor:" \pose;
        ENDIF
        
        !Which program to run
        runEGM;

        IF egmSt1 = EGM_STATE_CONNECTED THEN
            TPWrite "Reset EGM instance egmID1";
            EGMReset egmID1;    
        ENDIF        
    ENDPROC
        
        
    PROC runEGM()
        EGMActPose egmID1\Tool:=UISpenholder \WObj:=Kinect, posecorTable,EGM_FRAME_WORLD, posesenTable, EGM_FRAME_WORLD
        \x:=egm_minmax_lin1 \y:=egm_minmax_lin1 \z:=egm_minmax_lin1
        \rx:=egm_minmax_rot1 \ry:=egm_minmax_rot1 \rz:=egm_minmax_rot1\LpFilter:=5\Samplerate:=4\MaxSpeedDeviation:= 5000;
                
        EGMRunPose egmID1, EGM_STOP_HOLD\x \y \z\CondTime:=20 \RampInTime:=2\RampOutTime:=0.05;
        egmSt1:=EGMGetState(egmID1);   
    ENDPROC
 
ENDMODULE