using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

[Tool]
public partial class sprite2d_ui : Node2D
{

	#region ======== BaseVars ========

	//The base/background of the component
	private Sprite2D _base;
	[Export] private Texture2D _baseTexture;
	[Export] private Vector2 _baseScale = new Vector2(1f, 1f);
	[Export] private Vector2 _baseOffset = Vector2.Zero;
	[Export] private float _baseRotationOffset = 0f;
	[Export] private Color _baseColor0 = Color.FromHtml("#ffffff");
	[Export] private Color _baseColor100 = Color.FromHtml("#ffffff");

	#endregion

	#region ======== PipVars ========
	//All pips in the middle
	[Export] private Texture2D _pipTexture;
	//Setting a different value for _pipScale0 and _pipScale100 causes scale over distance
	[Export] private Vector2 _pipScale0 = new Vector2(1f, 1f);
	[Export] private Vector2 _pipScale100 = new Vector2(1f, 1f);
	//Setting a different value for _pipOffset0 and _pipOffset100 causes offSet over distance
	[Export] private Vector2 _pipOffset0 = Vector2.Zero;
	[Export] private Vector2 _pipOffset100 = Vector2.Zero;
	//The space between each pip
	[Export] private float _pipSpacing = 0.2f;
	[Export(PropertyHint.Range, "0, 360")] private float _pipRotationOffset = 0f;
	//How far from the _base the first pip will be placed
	[Export] private float _pipCentreOffset = 0f;
	//Will always show this number of pips even if _distance is 0
	[Export] private int _minPips = 0;
	[Export] private int _maxPips = 5;
	[Export(PropertyHint.None, hintString: "Color of the '_pipTexture' at 0%")] private Color _pipColor0 = Color.FromHtml("#ffffff");
	[Export(PropertyHint.None, hintString: "Color of the '_pipTexture' at 100%")] private Color _pipColor100 = Color.FromHtml("#ffffff");
	#endregion

	#region  ======== EndVars ========
	//Optional end
	private Sprite2D _end;
	[Export] private Texture2D _endTexture;
	[Export] private Vector2 _endScale = new Vector2(1f, 1f);
	[Export] private Vector2 _endOffset = Vector2.Zero;
	[Export(PropertyHint.Range, "0, 360")] private float _endRotationOffset = 0f;
	[Export(PropertyHint.None, "Color of the '_endTexture' at 0%")] private Color _endColor0 = Color.FromHtml("#ffffff");
	[Export(PropertyHint.None, "Color of the '_endTexture' at 100%")] private Color _endColorEnd100 = Color.FromHtml("#ffffff");
	[Export] private bool _showEndAt0 = true;
	[Export] private bool _showEndAt100 = true;
	#endregion

	#region ======== Settings ========

	public enum Mode
	{
		Fill, //This will show each pip along the way until 100 is reached
		Snap, //This will snap one pip at its absolute position from 0 to 100
		Slide //This will show one pip that moves from 0 to 100
	}
	[Export] private Mode _mode = Mode.Fill;

	//Should the component mirror Pips on Y?
	[Export] bool _mirror = false;
	[Export] float _mirrorRotationOffset = 0f;

	//Change angle over distance
	[Export] public bool _angleBasedOnDistance = false;
	[Export(PropertyHint.Range, "0, 360")] private float _minAngle = 0;
	[Export(PropertyHint.Range, "0, 360")] private float _maxAngle = 360;

	#endregion

	#region ======== RuntimeVars ========

	private List<Sprite2D> _pips = new List<Sprite2D>();
	private List<Sprite2D> _mirroPips = new List<Sprite2D>();
	private Vector2 _startPosition;
	private float _startRotation;
	private float _maxR = 6.28f;

	#endregion 

	#region ======== Public Vars ========
	//How far is the target 0 to 100% of max _distance
	//Once the component is setup this is the only thing you'd have to change from code
	[Export(PropertyHint.Range, "0, 100")] public float distance = 0;
	[Export(PropertyHint.Range, "0, 360")] public float angle = 180;
	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		createBase();
		_startPosition = _base.Position;
		_startRotation = _base.Rotation;

		for (int i = 0; i < _maxPips; i++)
		{
			createPip(i);

			if (_mirror)
			{
				createMirrorPip(i);
			}
		}

		creatEnd();
	}

	public override void _Process(double delta)
	{
		if (!safetyChecks()) return;

		int pipsToShow = Mathf.RoundToInt((distance / 100) * (_maxPips - _minPips)) + _minPips;

		processBase();
		processPips(pipsToShow);
		processEnd(pipsToShow);
	}

	/// <summary>
	/// This does cleanup to keep values in check if changed at runtime
	/// </summary>
	/// <returns>Returns true if all good to continue for current frame</returns>
	private bool safetyChecks()
	{
		//Some Clamps 
		if (_minPips < 0) _minPips = 0;
		if (_maxPips < 1) _maxPips = 1;
		if (_minPips > _maxPips) _maxPips = _minPips;
		if (_maxPips < _minPips) _minPips = _maxPips;

		//Cleanup mirror if it was disabled
		if (!_mirror && _mirroPips.Count > 0)
		{
			foreach (var mirror in _mirroPips)
			{
				mirror.Hide();
				mirror.Free();
			}
			_mirroPips.Clear();
		}

		//Make sure we have enough created pips if the max or minPips change at runtime
		if (_maxPips > _pips.Count)
		{
			//Add more pips
			for (int i = _pips.Count; i < _maxPips; i++)
			{
				createPip(i);
				if (_mirror == true)
				{
					createMirrorPip(i);
				}
			}
			return false;
		}

		if (_maxPips < _pips.Count)
		{
			//Remove pips
			for (int i = _pips.Count - _maxPips - 1; i < _pips.Count - 1; i++)
			{
				hidePip(i);
				destroyPip(i);
				return false;
			}
			return false;
		}
		return true;
	}

	#region ======== Base ========

	private void createBase()
	{
		_base = new Sprite2D();

		moveBase();
		scaleBase();
		rotateBase();		
		updateBaseColor();
		updateBaseTexture();	
		AddChild(_base);
	}

	private void processBase()
	{
		moveBase();
		scaleBase();
		rotateBase();
		updateBaseColor();
		updateBaseTexture();
	}

	private void moveBase()
	{
		_base.Position = _startPosition + _baseOffset;
	}

	private void scaleBase()
	{
		_base.Scale = _baseScale;
	}

	private void rotateBase()
	{
		if (_angleBasedOnDistance)
		{
			var lerpAngle = Mathf.Lerp(_minAngle, _maxAngle, Mathf.InverseLerp(0, 100, distance));
			_base.Rotation = Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, lerpAngle));
		}
		else
		{
			_base.Rotation = Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, angle));
		}

		_base.Rotation += Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _baseRotationOffset));
	}
	
	/// <summary>
	/// Changes the color of the base Sprite2D based on the distance from 0 to 100
	/// </summary>
	private void updateBaseColor()
	{
		if (_baseColor0 == _baseColor100)
		{
			_base.SelfModulate = _baseColor0;
		}
		else
		{
			var r = Mathf.Lerp(_baseColor0.R, _baseColor100.R, Mathf.InverseLerp(0, 100, distance));
			var g = Mathf.Lerp(_baseColor0.G, _baseColor100.G, Mathf.InverseLerp(0, 100, distance));
			var b = Mathf.Lerp(_baseColor0.B, _baseColor100.B, Mathf.InverseLerp(0, 100, distance));
			var a = Mathf.Lerp(_baseColor0.A, _baseColor100.A, Mathf.InverseLerp(0, 100, distance));
			_base.SelfModulate = new Color(r, g, b, a);
		}
	}

	private void updateBaseTexture()
	{
		if (_base.Texture != _baseTexture) _base.Texture = _baseTexture;
	}

	#endregion

	#region ======== Pips ========

	private void createPip(int i, bool startActive = false)
	{
		Sprite2D newPip = new Sprite2D
		{
			Texture = _pipTexture,
			Scale = _pipScale0,
			Rotation = _base.Rotation
		};

		if (_pipRotationOffset != 0)
		{
			newPip.Rotation += Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _pipRotationOffset));
		}

		newPip.Position = new Vector2(
			Position.X + _pipOffset0.X,
			Position.Y + _pipCentreOffset + (_pipSpacing * i) + _pipOffset0.Y) + new Vector2(
					Mathf.Lerp(_pipOffset0.X, _pipOffset100.X, Mathf.InverseLerp(0, _maxPips, i)),
					Mathf.Lerp(_pipOffset0.Y, _pipOffset100.Y, Mathf.InverseLerp(0, _maxPips, i))
				);

		_base.AddChild(newPip);
		_pips.Add(newPip);
		newPip.SetProcess(startActive);
		if (!startActive)
		{
			newPip.Hide();
		}
	}

	private void createMirrorPip(int i)
	{
		Sprite2D mirrorPip = new Sprite2D
		{
			Position = -_pips[i].Position,
			Texture = _pips[i].Texture,
			Modulate = _pips[i].Modulate,
			Scale = _pips[i].Scale,
			Rotation = -_pips[i].Rotation + Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _mirrorRotationOffset))
		};

		_base.AddChild(mirrorPip);
		_mirroPips.Add(mirrorPip);
	}

	private void destroyPip(int i)
	{
		if (_pips.Count > i + 1 && _pips[i].IsNodeReady())
		{
			_pips[i].QueueFree();
			_pips.RemoveAt(i);

			if (_mirroPips.Count > i)
			{
				destroyMirrorPip(i);
			}
		}
	}

	private void destroyMirrorPip(int i)
	{
		_mirroPips[i].QueueFree();
		_mirroPips.RemoveAt(i);
	}

	private void processPips(int pipsToShow)
	{
		if (pipsToShow < _minPips)
		{
			showMinPips();
		}
		else for (int i = 0; i < _maxPips; i++)
			{
				if (i > _pips.Count - 1)
				{
					continue;
				}
				if (i < pipsToShow)
				{
					renderPip(i, pipsToShow);
				}
				else
				{
					hidePip(i);
				}
			}
	}

	private void renderPip(int i, int pipsToShow)
	{
		//Recalculate spacing based on variable at runtime
		var pipIndex = 0;
		if (_mode == Mode.Snap || _mode == Mode.Slide)
		{
			hidePip(i);
		}
		else
		{
			pipIndex = i;
		}

		showPip(pipIndex);
		movePip(pipIndex, pipsToShow);
		scalePip(pipIndex);
		rotatePip(pipIndex);
		updatePipColor(pipIndex, pipsToShow);
		updatePipTexture(pipIndex);
	}

	/// <summary>
	/// This is used to show #minPips pips when #minpips > 0
	/// </summary>
	private void showMinPips()
	{
		for (int i = 0; i < _minPips; i++)
		{
			var pipIndex = 0;
			if (_mode == Mode.Snap || _mode == Mode.Slide)
			{
				hidePip(i);
			}
			else
			{
				pipIndex = i;
			}

			showPip(pipIndex);
			movePip(pipIndex, _minPips);
			scalePip(pipIndex);
			rotatePip(pipIndex);
			updatePipColor(pipIndex, _minPips);
			updatePipTexture(pipIndex);
		}
	}

	private void showPip(int i)
	{
		if (_pips.Count < i + 1) return;

		_pips[i].SetProcess(true);
		_pips[i].Show();

		if (_mirror)
		{
			showMirrorPip(i);
		}
	}
	private void showMirrorPip(int i)
	{
		if (_mirroPips.Count < i + 1)
		{
			createMirrorPip(i);
		}

		_mirroPips[i].SetProcess(true);
		_mirroPips[i].Show();
	}

	private void hidePip(int i)
	{
		if (_pips.Count < i + 1) return;

		_pips[i].Hide();
		_pips[i].SetProcess(false);

		hideMirrorPip(i);
	}
	private void hideMirrorPip(int i)
	{
		if (_mirroPips.Count < i + 1) return;

		_mirroPips[i].Hide();
		_mirroPips[i].SetProcess(false);
	}

	private void movePip(int i, int pipsToShow)
	{
		if (_mode == Mode.Slide)
		{
			slidePip(i, pipsToShow);
		}
		else
		{
			_pips[i].Position = new Vector2(
				Position.X,
				Position.Y + _pipCentreOffset + (_pipSpacing * (_mode == Mode.Snap ? pipsToShow - 1 : i))
			);

			_pips[i].Position +=
			new Vector2(
				Mathf.Lerp(_pipOffset0.X, _pipOffset100.X, Mathf.InverseLerp(0, _maxPips, _mode == Mode.Snap ? pipsToShow - 1 : i)),
				Mathf.Lerp(_pipOffset0.Y, _pipOffset100.Y, Mathf.InverseLerp(0, _maxPips, _mode == Mode.Snap ? pipsToShow - 1 : i))
			);
		}

		if (_mirror && i < _mirroPips.Count)
		{
			_mirroPips[i].Position = new Vector2(_pips[i].Position.X, -_pips[i].Position.Y);
		}
	}

	private void slidePip(int i, int pipsToShow)
	{
		if (i == 0)
		{
			_pips[i].Position = new Vector2(
			Position.X,
				Mathf.Lerp(
					Position.Y + _pipCentreOffset,
					Position.Y + _pipCentreOffset + _pipSpacing,
					Mathf.InverseLerp(0, 100 / _maxPips, distance))
			);
		}
		else
		{
			_pips[i].Position = new Vector2(
				Position.X,
				Mathf.Lerp(
					Position.Y + _pipCentreOffset + (_pipSpacing * i),
					Position.Y + _pipCentreOffset + (_pipSpacing * (i + 1)),
					Mathf.InverseLerp(100 - (i * 100 / _maxPips), 100 - (i * 100 / _maxPips) + (100 / _maxPips), distance))
			);
		}

		_pips[i].Position +=
		new Vector2(
			Mathf.Lerp(_pipOffset0.X, _pipOffset100.X, Mathf.InverseLerp(0, 100, distance)),
			Mathf.Lerp(_pipOffset0.Y, _pipOffset100.Y, Mathf.InverseLerp(0, 100, distance))
		);
	}

	private void scalePip(int i)
	{
		if (_pipScale0 == _pipScale100)
		{
			_pips[i].Scale = _pipScale0;
		}
		else
		{
			if (_mode != Mode.Slide && _mode != Mode.Snap)
			{
				_pips[i].Scale = new Vector2(
					Mathf.Lerp(_pipScale0.X, _pipScale100.X, Mathf.InverseLerp(0, _maxPips, i)),
					Mathf.Lerp(_pipScale0.Y, _pipScale100.Y, Mathf.InverseLerp(0, _maxPips, i)));
			}
			else
			{
				_pips[i].Scale = new Vector2(
					Mathf.Lerp(_pipScale0.X, _pipScale100.X, Mathf.InverseLerp(0, 100, distance)),
					Mathf.Lerp(_pipScale0.Y, _pipScale100.Y, Mathf.InverseLerp(0, 100, distance)));
			}
		}

		if (_mirror && i < _mirroPips.Count)
		{
			_mirroPips[i].Scale = _pips[i].Scale;
		}
	}

	private void rotatePip(int i)
	{
		_pips[i].Rotation = _startRotation;

		if (_pipRotationOffset != 0)
		{
			_pips[i].Rotation += Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _pipRotationOffset));
		}

		if (_mirror && i < _mirroPips.Count)
		{
			_mirroPips[i].Rotation = -_pips[i].Rotation;
			if (_mirrorRotationOffset != 0)
			{
				_pips[i].Rotation += Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _mirrorRotationOffset));
			}
		}
	}

	private void updatePipColor(int i, int pipsToShow)
	{
		if (_pipColor0 == _pipColor100)
		{
			_pips[i].Modulate = _pipColor0;
		}
		else
		{
			var r = Mathf.Lerp(_pipColor0.R, _pipColor100.R, Mathf.InverseLerp(0, _maxPips, _mode != Mode.Fill ? pipsToShow - 1 : i));
			var g = Mathf.Lerp(_pipColor0.G, _pipColor100.G, Mathf.InverseLerp(0, _maxPips, _mode != Mode.Fill ? pipsToShow - 1 : i));
			var b = Mathf.Lerp(_pipColor0.B, _pipColor100.B, Mathf.InverseLerp(0, _maxPips, _mode != Mode.Fill ? pipsToShow - 1 : i));
			var a = Mathf.Lerp(_pipColor0.A, _pipColor100.A, Mathf.InverseLerp(0, _maxPips, _mode != Mode.Fill ? pipsToShow - 1 : i));
			_pips[i].Modulate = new Color(r, g, b, a);
		}

		if (_mirror && i < _mirroPips.Count)
		{
			_mirroPips[i].Modulate = _pips[i].Modulate;
		}
	}
	
	private void updatePipTexture(int i)
	{
		if (_pips[i].Texture != _pipTexture) _pips[i].Texture = _pipTexture;

		if (_mirror && i < _mirroPips.Count)
		{
			_mirroPips[i].Texture = _pips[i].Texture;
		}
	}

	#endregion

	#region ======== End ========

	private void creatEnd()
	{
		_end = new Sprite2D
		{
			Texture = _endTexture,
			Scale = _endScale,
			Rotation = Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _endRotationOffset)),
			Position = new Vector2(
			Position.X + _pipSpacing + _endOffset.X,
			Position.Y + _pipSpacing + _endOffset.Y)
		};
		_base.AddChild(_end);
	}
	
	private void processEnd(int pipsToShow)
	{
		if (_end.Texture == null) return;

		if ((distance == 0 && !_showEndAt0) ||
			distance == 0 && _showEndAt0 && _showEndAt100)
		{
			hideEnd();
		}
		else
		{
			showEnd();
			moveEnd(pipsToShow);
			scaleEnd();
			rotateEnd();
			updateEndColor();
			updateEndTexture();			
		}
	}
	
	private void showEnd()
	{
		if (distance < 100 && _showEndAt100 && !_showEndAt0)
		{
			hideEnd();
			return;
		}

		if (distance > 0 && _showEndAt0 && !_showEndAt100)
		{
			hideEnd();
			return;
		}

		_end.Show();
	}

	private void hideEnd()
	{
		_end.Hide();
	}

	/// <summary>
	/// Updates the position of the end node in relation to the number of pips currenty shown
	/// </summary>
	/// <param name="pipsToShow">Number of pips to show in current update loop</param>
	private void moveEnd(int pipsToShow)
	{
		if (!_showEndAt100 || !_showEndAt0)
		{
			_end.Position = Position + _endOffset;

			return;
		}
		if (pipsToShow > 0)
		{
			//Now move the arrow to pipsToShow + 1 location
			_end.Position = new Vector2(
				Position.X + _endOffset.X,
				Position.Y + _pipCentreOffset + (_pipSpacing * pipsToShow) + _endOffset.Y) + _baseOffset;

		}
		else if (pipsToShow < _minPips)
		{
			_end.Position = new Vector2(Position.X + _endOffset.X,
				Position.Y + _pipCentreOffset + (_pipSpacing * _minPips) + _endOffset.Y) + _baseOffset;
		}
		else
		{
			_end.Position = new Vector2(Position.X + _endOffset.X, Position.Y + _pipCentreOffset + _endOffset.Y) + _baseOffset;
		}
	}

	private void scaleEnd()
	{
		_end.Scale = _endScale;
	}

	private void rotateEnd()
	{
		if (!_showEndAt100 || !_showEndAt0)
		{
			_end.Rotation = _base.Rotation + Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _endRotationOffset));

			return;
		}

		_end.Rotation = _startRotation + Mathf.Lerp(0f, _maxR, Mathf.InverseLerp(0, 360, _endRotationOffset));
	}

	private void updateEndColor()
	{
		if (_endColor0 == _endColorEnd100)
		{
			_end.Modulate = _endColor0;
		}
		else
		{
			var r = Mathf.Lerp(_endColor0.R, _endColorEnd100.R, Mathf.InverseLerp(0, 100, distance));
			var g = Mathf.Lerp(_endColor0.G, _endColorEnd100.G, Mathf.InverseLerp(0, 100, distance));
			var b = Mathf.Lerp(_endColor0.B, _endColorEnd100.B, Mathf.InverseLerp(0, 100, distance));
			var a = Mathf.Lerp(_endColor0.A, _endColorEnd100.A, Mathf.InverseLerp(0, 100, distance));
			_end.Modulate = new Color(r, g, b, a);
		}
	}

	/// <summary>
	/// Updates the texture of the end object if it changed at runtime
	/// </summary>
	private void updateEndTexture()
	{
		if (_endTexture != _end.Texture) _end.Texture = _endTexture;
	}
	
	#endregion
}
