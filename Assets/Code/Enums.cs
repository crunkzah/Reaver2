public enum EntityType : int 
{
      UNDEFINED_ENTITY,
      GLOCK_ENTITY,
      M416_ENTITY,
      SHOTGUN_ENTITY,
      CROSSBOW_ENTITY,
      HEAVYBOLTER_ENTITY,
      REVOLVER_ENTITY,
      ROCKETLAUNCHER_ENTITY,
      
      PISTOL_AMMO,
      RIFLE_AMMO,
      SHOTGUN_AMMO,
      CROSSBOW_AMMO,
      HEAVYBOLTER_AMMO,
      REVOLVER_AMMO,
      ROCKETLAUNCHER_AMMO,
      
      KEY_BLUE,
      KEY_RED,
      KEY_YELLOW,
}

public enum AmmoType : int 
{
      GlockAmmo,
      ShotgunAmmo,
      RifleAmmo,
      CrossbowAmmo,
      HeavyBolterAmmo,
      RevolverAmmo,
      RocketLauncherAmmo
}

public enum SceneNetObject : int 
{
     UNDEFINED,
     
     GLOCK_DROPPED,
     M416_DROPPED,
     MASTIFF_DROPPED,
     CROSSBOW_DROPPED,
     HEAVYBOLTER_DROPPED,
     
     PISTOL_AMMO_DROPPED,
     RIFLE_AMMO_DROPPED,
     SHOTGUN_AMMO_DROPPED,
     CROSSBOW_AMMO_DROPPED,
     HEAVYBOLTER_AMMO_DROPPED,
     
     REVOLVER_AMMO_DROPPED,
     REVOLVER_DROPPED,
     
     ROCKETLAUNCHER_AMMO_DROPPED,
     ROCKETLAUNCHER_DROPPED
}

public enum NetworkCommand : byte
{
     UNDEFINED_CMD,
     ResetRoomTimer,
     WakeUp,
     Sync,
     SwitchStateRemotely,
     Move,
     Attack,
     TakeDamage,
     Die,
     GetNotified,
     DoTrigger,
     SetTarget,
     ClearTarget,
     Slam,
     Shoot,
     Ability1,
     SetState,
     InteractCmd,
     GoToCover,
     Ability2,
     GlobalCommand,
     DieWithForce,
     LaunchAirborne,
     GroundSlam,
     Blink,
     LandOnGround,
     LaunchAirborneUp,
     Flee,
     OpenGates,
     CloseGates,
     ExplodeAsCorpse,
     LockGates,
     Ability3,
     ForceOpenGates,
}

//Object pool keys:
public enum ObjectPoolKey : int
{
     GlockBullet,
     ShotgunPellet,
     Bolt,
     HeavyBolterBullet,
     // RevolverBullet,
          
     Glock,
     Shotgun,
     Crossbow,
     HeavyBolter,
     
     Slammer,
     Rider,
     Boss1,
     
     GlockDropped,
     M416Dropped,
     ShotgunDropped,
     CrossbowDropped,
     HeavyBolterDropped,
     
     PistolAmmoDropped,
     RifleAmmoDropped,
     ShotgunAmmoDropped,
     CrossbowAmmoDropped,
     HeavyBolterAmmoDropped,
     
     Bomber,
     Bomb1,
     
     Bullet_npc1,
     BomberHalf,
     Bullet_npc2,
     Bullet_npc3,
     RobotBasicShooter_npc,
     
     InteractableFancy1,
     RobotBasicShooter_shotgun_npc,
     Gear_harmful1,
     Skull1,
     RobotStationary1,
     Bullet_npc6,
     Kaboom1,
     FlyingGib1,
     
     Revolver,
     RevolverDropped,
     RevolverAmmoDropped,
     RevolverBullet,
     BloodPuddle_1,
     
     RocketLauncher,
     RocketLauncherDropped,
     RocketLauncherAmmoDropped,
     RocketLauncher_rocket,
     
     RobotCharger,
     Chaser2,
     Chaser2_ragdoll,
     Shooter2,
     Shooter3_shotgunner,
     Witch,
     ShootyThing_bullet1,
     Revolver_bullet_alt,
     DeferredShooter,
     AR_Bullet,
     HealthCrystal,
     RevolverBullet2,
     ShootyThing_bullet2,
     Revolver_bullet_strong,
     Revolver_bullet_ult,
     Blood_stain1,
     Blood_stain2,
     Blood_stain3,
     Blood_stain4,
     BloodSprayer,
     FlyingDagger1,
     DeferredGroundSlam,
     Shotgun_bullet_hurtless,
     HealthCrystal_smaller,
     Revolver_bullet_reflective,
     Shotgun_bullet_alt,
     Olios_direct_projectile,
     LightPooled,
     RocketLauncher_alt_projectile,
     Bullet_yellow,
     mp5_bullet,
     mp5_grenade
}

public enum FPS_Func : byte
{
     Shoot_revolver,
     Shoot_revolver_stronger,
     Shoot_revolver_ult,
     Shoot_AR,
     Shoot_AR_Ghost,
     Shoot_shotgun,
     Shoot_shotgun_alt,
     Shoot_rocketLauncher,
     Shoot_rocketLauncher_alt,
     Shoot_mp5,
     Shoot_mp5_grenade,
     Punch1,
     Punch1_ult
}

public enum PlayerAnimationCommand : int
{
     UNWIELD,
     Wield_Pistol,
     Wield_Shotgun,
     Wield_Crossbow,
     Wield_HeavyBolter,
     Wield_Revolver,
     Wield_RocketLauncher
}

public enum GunAnimationCommand : int
{
     GlockFire
}

public enum RevolverState : int
{
     Normal,
     Charging,
     Ult
}


public enum TriggerTarget : int
{
     LocalPlayer
}

public enum LeverState : int
{
     Off,
     On
}

public static class EventCodes
{
     public const byte GiveItem = 1;
}

