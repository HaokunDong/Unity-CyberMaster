using UnityEngine;

public class BetterJumping
{
    private float gate;
    private float fallMultiplier;
    private float lowJumpMultiplier;
    
    private Rigidbody2D rb;
    private PlayerInput input;

    public BetterJumping(float g, float f, float l, Rigidbody2D rb, PlayerInput input)
    {
        gate = g;
        fallMultiplier = f;
        lowJumpMultiplier = l;
        this.rb = rb;
        this.input = input;
    }

    public void Step()
    {
        if (rb.velocity.y < -gate)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > gate && !input.GamePlay.Jump.IsPressed())
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
}