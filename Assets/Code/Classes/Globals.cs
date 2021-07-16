using UnityEngine;


public static class Ghoul
{
    public static int maxHealth            = 250;
    
    
    public const float angularSpeed         = 1440f * 2f;
    public const float acceleration         = 60f;
    public const float stoppingDistance     = 1.9f;
    
    public const float idleMoveSpeed        = 4.5f;
    public const float idleVisionRange      = 10f;
    public const float idlePosChangeRateMin = 2f;
    public const float idlePosChangeRateMax = 6f;
    
    public const float curiousMoveSpeed     = 6f;
    public const float curiousVisionRange   = 16f;
    public const float curiousDuration      = 6f;
        
    public const float chasingMoveSpeed     = 6f;
    public const float chasingVisionRange   = 8f;
    public const float chasingPlayerNotSeenTimeout = 3f;
    
    public const float pounceCooldown       = 4f;
    public const float pounceSpeed          = 2.5f;
    public const float pounceYApex          = 4f;
    public const float pounceYBottom        = 0.5f;
    public const float pounceMinDist        = 8f;
    public const float pounceMaxDist        = 35f;
    public const float pounceSqrMinDist     = pounceMinDist * pounceMinDist;
    public const float pounceSqrMaxDist     = pounceMaxDist * pounceMaxDist;
    
    
    
}

public static class RobotWorker
{
    public static float MoveSpeed   = 6f;
    public static float JumpLength  = 2.2f;
    public static float JumpHeight  = 1.1f;
}

public static class NPC
{
    public static void ChooseTarget()
    {
        
    }
}

public static class Globals
{
    public const float npcGravityMultiplier = 1.4f;
    public const float AI_update_rate = 0.03f;
    
    public const float Gravity = -9.81F;
    
    public static bool DEBUGBreakPoint = false;
    
    public readonly static Vector3 playerEyesOffset = new Vector3(0f, 0.0f, 0f);
    
    public const float NPC_airbourne_force_mult = 1.5F;
    public const float NPC_gravity = -15F;
}

public static class QuaternionUtil 
{
	
	public static Quaternion AngVelToDeriv(Quaternion Current, Vector3 AngVel) {
		var Spin = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f);
		var Result = Spin * Current;
		return new Quaternion(0.5f * Result.x, 0.5f * Result.y, 0.5f * Result.z, 0.5f * Result.w);
	} 

	public static Vector3 DerivToAngVel(Quaternion Current, Quaternion Deriv) {
		var Result = Deriv * Quaternion.Inverse(Current);
		return new Vector3(2f * Result.x, 2f * Result.y, 2f * Result.z);
	}

	public static Quaternion IntegrateRotation(Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime) {
		if (DeltaTime < Mathf.Epsilon) return Rotation;
		var Deriv = AngVelToDeriv(Rotation, AngularVelocity);
		var Pred = new Vector4(
				Rotation.x + Deriv.x * DeltaTime,
				Rotation.y + Deriv.y * DeltaTime,
				Rotation.z + Deriv.z * DeltaTime,
				Rotation.w + Deriv.w * DeltaTime
		).normalized;
		return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
	}
	
	public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
		if (Time.deltaTime < Mathf.Epsilon) return rot;
		// account for double-cover
		var Dot = Quaternion.Dot(rot, target);
		var Multi = Dot > 0f ? 1f : -1f;
		target.x *= Multi;
		target.y *= Multi;
		target.z *= Multi;
		target.w *= Multi;
		// smooth damp (nlerp approx)
		var Result = new Vector4(
			Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
			Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
			Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
			Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
		).normalized;
		
		// ensure deriv is tangent
		var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
		deriv.x -= derivError.x;
		deriv.y -= derivError.y;
		deriv.z -= derivError.z;
		deriv.w -= derivError.w;		
		
		return new Quaternion(Result.x, Result.y, Result.z, Result.w);
	}
}

public static class NPCTool
{
    public static Vector3 GetCapsuleBottomPoint(Transform tr, CapsuleCollider capsule)
    {
        Vector3 Result = tr.localPosition;
        
        Result.y += capsule.center.y - capsule.height/2 + capsule.radius * 0.9f + 0.1f;
        
        return Result;
    }
    public static Vector3 GetCapsuleTopPoint(Transform tr, CapsuleCollider capsule)
    {
        Vector3 Result = tr.localPosition;
        
        Result.y += capsule.center.y + capsule.height/2 - capsule.radius * 0.9f - 0.1f;
        
        return Result;
    }
}

public static class Math
{
    
    const float trig45 = 0.7071067F;
    
    public static Vector3[] shotDirRadial8 = 
    {
        new Vector3(-1, 0, 0), new Vector3(-trig45, trig45, 0), new Vector3(0, 1, 0),
        new Vector3(trig45, trig45, 0), new Vector3(1, 0, 0), new Vector3(trig45, -trig45, 0),
        new Vector3(0, -1, 0), new Vector3(-trig45, -trig45, 0)
    };
    
    public const float DistanceError = 0.007f;
    public const float SqrDistanceError = 0.007f * 0.007f;
        
    public static float SqrMagnitude(Vector3 v)
    {
        return v.x * v.x + v.y * v.y + v.z * v.z;
    }
    
    public static Vector2 SolveQuad(float a, float b, float c)
    {
        //ax^2 + bx + c = 0
        float x1, x2;
        
        float d = b * b - 4 * a * c;
        
        if(d < 0)
        {
            InGameConsole.LogOrange(string.Format("<color=yellow>{0}x^2 + {1}x + {2}</color> Discriminant less than zero!!!", a.ToString("f"), b.ToString("f"), c.ToString("f")));
            return new Vector2(-99, -99);
        }
        
        x1 = x2 = -b/(2 * a);
        float dSqrt = Mathf.Sqrt(d);
        
        float dSqrt_ = dSqrt / (2*a);
        
        x1 += dSqrt_;
        x2 -= dSqrt_;
        
        return new Vector2(x1, x2);
    }
    
    public static float Max(float a, float b)
    {
        return a > b ? a : b;
    }
    
    public static float Min(float a, float b)
    {
        return a < b ? a : b;
    }
    
    public static float Magnitude(Vector3 v)
    {
        float Result = SqrMagnitude(v);
        
        return Mathf.Sqrt(Result);
    }
    
    public static Vector3 Normalized(Vector3 v)
    {
        Vector3 Result = v;
        float sqrMagnitude = v.x * v.x + v.y * v.y + v.z * v.z;
        
        if(sqrMagnitude != 0f)
        {
            float magnitude = Mathf.Sqrt(sqrMagnitude);
            
            Result.x /= magnitude;
            Result.y /= magnitude;
            Result.z /= magnitude;
        }
        
        return (Result);
    }
    
    public static Vector3 GetXZ(Vector3 v)
    {
        v.y = 0f;
        return v;
    }
    
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        Vector3 Result;
        Result.x = Lerp(a.x, b.x, t);
        Result.y = Lerp(a.y, b.y, t);
        Result.z = Lerp(a.z, b.z, t);
        
        return Result;
    }
    
    public static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
    
    
    public static float SqrDistance(Vector3 a, Vector3 b)
    {
        float x = a.x - b.x;
        float y = a.y - b.y;
        float z = a.z - b.z;
        
        return x * x + y * y + z * z;
    }
    
    
    public static float BounceClampTop(float x)
    {
        return 1f - Abs(1f - x);
    }
    
    public static float BounceClampBottomTop(float x)
    {
        return BounceClampTop(Abs(x));
    }
    
    public static float Clamp01(float x)
    {
        if(x >= 1f)
        {
           x = 1f;
        }
        else
            if(x <= 0f)
            {
                x = 0f;
            }
        return x;
    }
    
    public static float UglyBounce(float x)
    {
        if(x > 0.775f)
        {
            return (4.5f * x - 3.985f) * (4.5f * x - 3.985f) + 0.75f;
        }
        else
        {
            return 1/(1 - x*x/1.2f) - 1;
        }
    }
    
    public static float SmoothHyperbola1(float x)
    {
        return (1/(1 - x/2) - 1);
    }
    
    public static float SmoothHyperbola2(float x)
    {
        return (1/(1 - x*x/2) - 1);
    }
    
    public static float Parabola01Negative(float x)
    {
        return (1f-(2f*x - 1f)*(2f*x - 1f));
    }
    public const float SqrtOf2 = 1.41421356237F;
    
    public const float NormalizeFactor = 0.7071F;
    
    public static void MakeVectorZero(ref Vector3 v)
    {
        v.x = v.y = v.z = 0f;
    }
    
    public static Vector3 GetVectorRotatedXZ(Vector3 v, float angle)
    {
        Vector3 Result = v;
        
        angle *= Mathf.Deg2Rad;
        
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);
        
        Result.x = v.x * cos - v.z * sin;
        Result.z = v.x * sin + v.z * cos; 
        
        return Result;
    }
    
    public static Vector3 GetVectorRotatedYZ(Vector3 v, float angle)
    {
        Vector3 Result = v;
        
        angle *= Mathf.Deg2Rad;
        
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);
        
        Result.y = v.y * cos - v.z * sin;
        Result.z = v.x * sin + v.z * cos; 
        
        return Result;
    }
    
    public static Vector2 GetVectorRotated(Vector2 v, float angle)
    {
        angle *= Mathf.Deg2Rad;
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        Vector2 r = new Vector2();
        r.x = v.x * cos - v.y * sin;
        //r.y = v.y;
        r.y = v.x * sin + v.y * cos;

        return r;
    }
    
    public static float Abs(float x)
    {
        
        if(x > 0f)
        {
            return x;
        }
        else
        {
            return -x;
        }
    }
    
    public static void Clamp(float min, float max, ref float value)
    {
        if(value <= min)
        {
            value = min;
        }
        else
        {
            if(value >= max)
            {
                value = max;
            }
        }
    }
    
    public static float Clamp(float min, float max, float val)
    {
        if(val >= max)
        {
            val = max;
        }
        else
        {
            if(val <= min)
            {
                val = min;
            }
        }
        
        return val;
    }
    
    public const float Sin48Deg = 0.7431F;
    
    
    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        float toV_x = target.x - current.x;
        float toV_y = target.y - current.y;
        float toV_z = target.z - current.z;

        float sqdist = toV_x * toV_x + toV_y * toV_y + toV_z * toV_z;

        if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
            return target;
            
        float dist = Mathf.Sqrt(sqdist);
        
        float dist_mult_maxDistanceDelta = dist * maxDistanceDelta;

        return new Vector3(current.x + toV_x / dist_mult_maxDistanceDelta,
                           current.y + toV_y / dist_mult_maxDistanceDelta,
                           current.z + toV_z / dist_mult_maxDistanceDelta);
    }

    public static float LinearToQuadratic(float linear_x)
    {
        linear_x = Mathf.Clamp(linear_x, 0f, 1f);
        float result = linear_x * linear_x;
        return result;
    }
    
}

public static class Colors
{
    public static Color Orange      = new Color(255f/255f, 157f/255f, 71f/255f);
    public static Color LightBlue   = new Color(3f/255f, 173f/255f, 252f/255f);
    public static Color Indigo      = new Color(75f/255f, 0f, 130f/255f);
    public static Color Lime        = new Color(204f/255f, 1f, 51f/255f);
    
    
    public static string OrangeHex    = "#ff5100";
    public static string LightBlueHex = "#03adfc";
    public static string IndigoHex    = "#4B0082";
    public static string LimeHex      = "#ccff33";
}



//RPC shortcuts:

//
