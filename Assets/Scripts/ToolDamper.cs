//--------------------------------------------------------------------
// Ported by Alexis Bacot - 2021 - www.alexisbacot.com
// From Daniel Holden (https://twitter.com/anorangeduck)
// Original article http://theorangeduck.com/page/spring-roll-call#damper
//--------------------------------------------------------------------
using UnityEngine;

//--------------------------------------------------------------------
public class ToolDamper
{
    //--------------------------------------------------------------------
    public static float damper(float x, float g, float halflife, float dt)
    {
        return Mathf.Lerp(x, g, 1.0f - fast_negexp((0.69314718056f * dt) / (halflife + Mathf.Epsilon)));
    }

    //--------------------------------------------------------------------
    private static float fast_negexp(float x)
    {
        return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
    }

    //--------------------------------------------------------------------
    private static float squaref(float x) { return x * x; }

    //--------------------------------------------------------------------
    /// <summary>
    /// Damper spring
    /// </summary>
    /// <param name="frequency">how many oscillations are done</param>
    /// <param name="halflife">how quick the goal is reached</param>
    public static void damper_spring(ref float x, ref float v, float x_goal, float v_goal, float frequency, float halflife, float dt)
    {
        float g = x_goal;
        float q = v_goal;
        float s = frequency_to_stiffness(frequency);
        float d = halflife_to_damping(halflife);
        float c = g + (d * q) / (s + Mathf.Epsilon);
        float y = d / 2.0f;

        if (Mathf.Abs(s - (d * d) / 4.0f) < Mathf.Epsilon) // Critically Damped
        {
            float j0 = x - c;
            float j1 = v + j0 * y;

            float eydt = fast_negexp(y * dt);

            x = j0 * eydt + dt * j1 * eydt + c;
            v = -y * j0 * eydt - y * dt * j1 * eydt + j1 * eydt;
        }
        else if (s - (d * d) / 4.0f > 0.0) // Under Damped
        {
            float w = Mathf.Sqrt(s - (d * d) / 4.0f);
            float j = Mathf.Sqrt(squaref(v + y * (x - c)) / (w * w + Mathf.Epsilon) + squaref(x - c));
            float p = Mathf.Atan((v + (x - c) * y) / (-(x - c) * w + Mathf.Epsilon));

            j = (x - c) > 0.0f ? j : -j;

            float eydt = fast_negexp(y * dt);

            x = j * eydt * Mathf.Cos(w * dt + p) + c;
            v = -y * j * eydt * Mathf.Cos(w * dt + p) - w * j * eydt * Mathf.Sin(w * dt + p);
        }
        else if (s - (d * d) / 4.0f < 0.0) // Over Damped
        {
            float y0 = (d + Mathf.Sqrt(d * d - 4 * s)) / 2.0f;
            float y1 = (d - Mathf.Sqrt(d * d - 4 * s)) / 2.0f;
            float j1 = (c * y0 - x * y0 - v) / (y1 - y0);
            float j0 = x - j1 - c;

            float ey0dt = fast_negexp(y0 * dt);
            float ey1dt = fast_negexp(y1 * dt);

            x = j0 * ey0dt + j1 * ey1dt + c;
            v = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
        }
    }

    //--------------------------------------------------------------------
    /// <summary>
    /// Damper spring set to critical (w = 0) spring goes to goal as fast as possible without oscillations, frequency is set by halflife
    /// </summary>
    public static void damper_spring_critical(ref float x, ref float v, float x_goal, float v_goal, float halflife, float dt)
    {
        float g = x_goal;
        float q = v_goal;
        float d = halflife_to_damping(halflife);
        float c = g + (d * q) / ((d * d) / 4.0f);
        float y = d / 2.0f;
        float j0 = x - c;
        float j1 = v + j0 * y;
        float eydt = fast_negexp(y * dt);

        x = eydt * (j0 + j1 * dt) + c;
        v = eydt * (v - j1 * y * dt);
    }

    //--------------------------------------------------------------------
    /// <summary>
    /// Damper spring set to critical AND we assume the goal has no speed, very smooth! smoothness similar to unity smoothdamping
    /// </summary>
    public static void damper_spring_critical_noGoalSpeed( ref float x, ref float v, float x_goal, float halflife, float dt)
    {
        float y = halflife_to_damping(halflife) / 2.0f;
        float j0 = x - x_goal;
        float j1 = v + j0 * y;
        float eydt = fast_negexp(y * dt);

        x = eydt * (j0 + j1 * dt) + x_goal;
        v = eydt * (v - j1 * y * dt);
    }

    //--------------------------------------------------------------------
    /// <summary>
    /// Double Damper spring set to critical, we assume goal has no speed
    /// Both follower and goal are damped, to achieve maximum smoothness
    /// </summary>
    public static void double_spring_damper_implicit( ref float x, ref float v, ref float xi, ref float vi, float x_goal, float halflife, float dt)
    {
        damper_spring_critical_noGoalSpeed(ref xi, ref vi, x_goal, 0.5f * halflife, dt);
        damper_spring_critical_noGoalSpeed(ref x, ref v, xi, 0.5f * halflife, dt);
    }

    //--------------------------------------------------------------------
    /*public static void timed_spring_damper_implicit( ref float x, ref float v, ref float xi, float x_goal, float t_goal, float halflife, float dt, float apprehension = 2.0f)
    {
        float min_time = t_goal > dt ? t_goal : dt;

        float v_goal = (x_goal - xi) / min_time;

        float t_goal_future = dt + apprehension * halflife;
        float x_goal_future = t_goal_future < t_goal ?
            xi + v_goal * t_goal_future : x_goal;

        damper_spring_critical_noGoalSpeed(ref x, ref v, x_goal_future, halflife, dt);

        xi += v_goal * dt;
    }*/

    //--------------------------------------------------------------------
    public static float critical_halflife(float frequency)
    {
        return damping_to_halflife(Mathf.Sqrt(frequency_to_stiffness(frequency) * 4.0f));
    }

    //--------------------------------------------------------------------
    public static float critical_frequency(float halflife)
    {
        return stiffness_to_frequency(squaref(halflife_to_damping(halflife)) / 4.0f);
    }

    //--------------------------------------------------------------------
    private static float frequency_to_stiffness(float frequency)
    {
        return squaref(2.0f * Mathf.PI * frequency);
    }

    //--------------------------------------------------------------------
    private static float stiffness_to_frequency(float stiffness)
    {
        return Mathf.Sqrt(stiffness) / (2.0f * Mathf.PI);
    }

    //--------------------------------------------------------------------
    private static float halflife_to_damping(float halflife)
    {
        return (4.0f * 0.69314718056f) / (halflife + Mathf.Epsilon);
    }

    //--------------------------------------------------------------------
    private static float damping_to_halflife(float damping)
    {
        return (4.0f * 0.69314718056f) / (damping + Mathf.Epsilon);
    }

    //--------------------------------------------------------------------
}

//--------------------------------------------------------------------
