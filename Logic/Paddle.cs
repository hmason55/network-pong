using Godot;
using System;

public class Paddle : Area2D
{
	// All three of these change for each paddle.
	private int _ballDir;

	public Paddle()
    {
    }

	public Paddle(Vector2 position)
    {
		Position = position;
	}

	public void SetLabel(string text)
    {
		PrintTreePretty();
		GetNode<Label>("Label").Text = text;
	}

	public override void _Ready()
	{
		string name = Name.ToLower();
		_ballDir = name == "left" ? 1 : -1;
	}


}
