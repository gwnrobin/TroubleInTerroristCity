public class Humanoid : Entity
{
    public readonly Value<float> MovementSpeedFactor = new Value<float>(1f);

    public readonly Value<Item> EquippedItem = new Value<Item>(null);
    public readonly Value<EquipmentItem> ActiveEquipmentItem = new Value<EquipmentItem>();

    public readonly Value<bool> Interact = new Value<bool>();

    /// <summary>
    /// <para>item - item to equip</para>
    /// <para>bool - do it instantly?</para>
    /// </summary>
    public readonly Attempt<Item, bool> EquipItem = new Attempt<Item, bool>();
    public readonly Attempt<Item> SwapItem = new Attempt<Item>();
    public readonly Attempt<Item> DropItem = new Attempt<Item>();

    /// <summary>
    /// <para>Use held item.</para>
    /// <para>bool - continuosly.</para>
    /// int - use type
    /// </summary>
    public readonly Activity UseItemHeld = new Activity();
    public readonly Attempt<bool, int> UseItem = new Attempt<bool, int>();

    public readonly Activity Walk = new Activity();
    public readonly Activity Sprint = new Activity();
    public readonly Activity Crouch = new Activity();
    public readonly Activity<float> Lean = new Activity<float>();
    public readonly Activity Prone = new Activity();

    public readonly Activity Jump = new Activity();

    public readonly Activity Aim = new Activity();
    public readonly Activity PointAim = new Activity();
    public readonly Activity Reload = new Activity();
    public readonly Activity Healing = new Activity();

    public readonly Activity Holster = new Activity();
    public readonly Attempt ChangeScope = new Attempt();
}