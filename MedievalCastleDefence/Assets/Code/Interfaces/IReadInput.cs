using Fusion;
public interface IReadInput
{
    [Networked] public NetworkButtons PreviousButton { get; set; }
    public void ReadPlayerInputs(PlayerInputData input);   
}
