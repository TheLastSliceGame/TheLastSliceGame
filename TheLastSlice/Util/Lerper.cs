using System;

namespace com.bitbull.meat
{
    /**
	 *  Lerper class for motion smoothing written by James Closs 12.01.20017
	 * 
	 *  Please use as you feel fit - pay me back with some good karma or buying my games!
	 * 
	 *  Twitter: @bitbulldotcom
	 *  
	 *  http://www.bitbull.com http://blog.bitbull.com
	 */

    public class Lerper
    {
        private float previous_velocity;
        public delegate void OnTargetDelegate();

        public Lerper()
        {
            Amount = 0.025f;
            Acceleration = float.MaxValue;
            MinVelocity = 0;
            MaxVelocity = float.MaxValue;
        }

        // Returns the amount of movement at this staget of the lerp
        private float LerpVelocity(float position, float target)
        {
            return (target - position) * Amount;
        }

        // Returns the next position with lerp smoothing
        public float Lerp(float position, float target)
        {
            // get the amount to move
            float v = LerpVelocity(position, target);
            // if its zero just return
            if (v == 0) return target;
            // store this value
            float vo = v;

            // don't allow increases in velocity beyond the specifed acceleration (ease in)
            // this also makes for smooth changes when switching direction
            //
            // only bother doing this if we're speeding up or changing direction
            // because the lerp takes care of the smoothing when slowing down
            //
            // note that multiplying two numbers together to check whether they are both
            // positive or negative is prone to overflow errors but as this class will 
            // realistically never be used for such massive numbers we should be OK! 
            if (v * previous_velocity < 0 || Math.Abs(v) > Math.Abs(previous_velocity))
            {
                if ( /*v>0 && previous_velocity>=0 &&*/ v - previous_velocity > Acceleration)
                {
                    v = previous_velocity + Acceleration;
                }
                else if ( /*v < 0 && previous_velocity <= 0 &&*/ previous_velocity - v > Acceleration)
                {
                    v = previous_velocity - Acceleration;
                }
            }

            // If this is less than the minimum velocity then
            // clamp at minimum velocity
            if (Math.Abs(v) < MinVelocity)
            {
                v = (vo > 0) ? MinVelocity : 0 - MinVelocity;
            }
            // If this is more than the maximum velocity then
            // clamp at maximum velocity
            else if (Math.Abs(v) > MaxVelocity)
            {
                v = (vo > 0) ? MaxVelocity : 0 - MaxVelocity;
            }
            // Remember the previous velocity
            previous_velocity = v;

            // Adjust the position based on the new velocity
            position += v;
            // Now account for potential overshoot and clamp to target if necessary
            if ((vo < 0 && position <= target) || (vo > 0 && position >= target))
            {
                position = target;
                if (OnReachedTarget != null) OnReachedTarget();
            }
            return position;
        }

        // Set a delegate to be called when the object reaches its destination
        public OnTargetDelegate OnReachedTarget
        {
            get;
            set;
        }

        // The Lerp amount - the larger this is the faster the object will move
        // Don't set it > 0 and probably start with values in the range 0.01->0.1 
        public float Amount
        {
            get;
            set;
        }

        // The minimum velocity the object will mover per frame. Defaults to zero.
        // 1 is usually a good choice for 1:1 onscreen movement
        public float MinVelocity
        {
            get;
            set;
        }
        // The maximum velocity the object will move per frame
        public float MaxVelocity
        {
            get;
            set;
        }
        // The maximum amount by which the object can accelerate per frame (or
        // deccelerate when changing direction)
        public float Acceleration
        {
            get;
            set;
        }
    }
}