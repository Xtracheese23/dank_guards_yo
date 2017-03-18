using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PID : MonoBehaviour {
    private float integral = 0F;
    private float prev_error = 0F;
    private float Kp, Ki, Kd;

    float PIDs(float error)
    {
        integral += integral + (error * Time.deltaTime);
        var derivative = (error - prev_error) / Time.deltaTime;
        var output = Kp * error + Ki * integral + Kd * derivative;
        prev_error = error;
        //sleep(iteration_time)
        return output;
    }

}
