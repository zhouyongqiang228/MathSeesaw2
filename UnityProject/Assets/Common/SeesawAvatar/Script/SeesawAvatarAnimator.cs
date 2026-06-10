using UnityEngine;

public class SeesawAvatarAnimator : MonoBehaviour
{
    public const string WalkState = "walk";
    public const string WalkWithObjectState = "walkWithObj";
    public const string PatrolState = "patrol";
    public const string RunState = "running";
    public const string RunAltState = "running2";
    public const string IdleState = "idle";
    public const string DanceState = "dancing";
    public const string VictoryState = "victory";
    public const string LoseState = "lose";
    public const string StickRunState = "runningstick";
    public const string FightState = "fight";
    public const string DyingState = "dying";
    public const string SitState = "sit";
    public const string ClimbState = "climb";
    public const string JumpState = "jump";
    public const string KissState = "kiss";
    public const string DieAltState = "die2";
    public const string HitReactionState = "beattack";
    public const string FightIdleState = "fightidle";
    public const string ShootState = "shoot";
    public const string SittingIdleState = "sittingidle";
    public const string SittingGreetingState = "sittinghi";

    [SerializeField] private Animator avatarAnimator;

    [SerializeField] private SkinnedMeshRenderer avatarRenderer;

    public Animator Animator => avatarAnimator;
    public SkinnedMeshRenderer MeshRenderer => avatarRenderer;

    public void PlayWalk() => PlayState(WalkState);

    public void PlayWalkWithObject() => PlayState(WalkWithObjectState);

    public void PlayPatrol() => PlayState(PatrolState);

    public void PlayIdle() => PlayState(IdleState);

    public void PlayFight() => PlayState(FightState);

    public void PlayStickRun() => PlayState(StickRunState);

    public void PlayRun() => PlayState(RunState);

    public void PlayRunAlternate() => PlayState(RunAltState);

    public void PlayDance() => PlayState(DanceState);

    public void PlayVictory() => PlayState(VictoryState);

    public void PlayLose() => PlayState(LoseState);

    public void PlayDying() => PlayState(DyingState);

    public void PlaySit() => PlayState(SitState);

    public void PlayClimb() => PlayState(ClimbState);

    public void PlayJump() => PlayStateFromStart(JumpState);

    public void PlayKiss() => PlayState(KissState);

    public void PlayDieAlternate() => PlayState(DieAltState);

    public void PlayHitReaction() => PlayState(HitReactionState);

    public void PlayFightIdle() => PlayState(FightIdleState);

    public void PlayShoot() => PlayState(ShootState);

    public void SetPlaybackSpeed(float speed)
    {
        avatarAnimator.speed = speed;
    }

    public void SetPaused(bool isPaused)
    {
        avatarAnimator.enabled = !isPaused;
    }

    public void PlayState(string stateName)
    {
        avatarAnimator.Play(stateName);
    }

    public void PlayStateFromStart(string stateName)
    {
        avatarAnimator.Play(stateName, 0, 0);
        avatarAnimator.Update(0);
    }

    public void setWalk() => PlayWalk();

    public void setWalkWithObj() => PlayWalkWithObject();

    public void setPatrol() => PlayPatrol();

    public void setIdle() => PlayIdle();

    public void setFight() => PlayFight();

    public void setRunningStick() => PlayStickRun();

    public void setRunning() => PlayRun();

    public void setRunning2() => PlayRunAlternate();

    public void setDancing() => PlayDance();

    public void setVictory() => PlayVictory();

    public void setLose() => PlayLose();

    public void setDying() => PlayDying();

    public void setSit() => PlaySit();

    public void setClimb() => PlayClimb();

    public void setJump() => PlayJump();

    public void setKiss() => PlayKiss();

    public void setDie2() => PlayDieAlternate();

    public void setBeAttack() => PlayHitReaction();

    public void setFightIdle() => PlayFightIdle();

    public void setShoot() => PlayShoot();

    public void setAniSpeed(float aniSpeed) => SetPlaybackSpeed(aniSpeed);

    public void setPause(bool p) => SetPaused(p);
}
