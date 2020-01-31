using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SamEngine;

namespace GooseDesktop
{
	// Token: 0x0200000C RID: 12
	internal static class TheGoose
	{
		// Token: 0x06000048 RID: 72 RVA: 0x000033EC File Offset: 0x000015EC
		public static void Init()
		{
			TheGoose.position = new Vector2(-20f, 120f);
			TheGoose.targetPos = new Vector2(100f, 150f);
			if (!GooseConfig.settings.CanAttackAtRandom)
			{
				int num = Array.IndexOf<int>(TheGoose.taskPickerDeck.indices, Array.IndexOf<TheGoose.GooseTask>(TheGoose.gooseTaskWeightedList, TheGoose.GooseTask.CollectWindow_Meme));
				int num2 = TheGoose.taskPickerDeck.indices[0];
				TheGoose.taskPickerDeck.indices[0] = TheGoose.taskPickerDeck.indices[num];
				TheGoose.taskPickerDeck.indices[num] = num2;
			}
			TheGoose.lFootPos = TheGoose.GetFootHome(false);
			TheGoose.rFootPos = TheGoose.GetFootHome(true);
			TheGoose.shadowBitmap = new Bitmap(2, 2);
			TheGoose.shadowBitmap.SetPixel(0, 0, Color.Transparent);
			TheGoose.shadowBitmap.SetPixel(1, 1, Color.Transparent);
			TheGoose.shadowBitmap.SetPixel(1, 0, Color.Transparent);
			TheGoose.shadowBitmap.SetPixel(0, 1, Color.DarkGray);
			TheGoose.shadowBrush = new TextureBrush(TheGoose.shadowBitmap);
			TheGoose.shadowPen = new Pen(TheGoose.shadowBrush);
			Pen pen = TheGoose.shadowPen;
			TheGoose.shadowPen.EndCap = LineCap.Round;
			pen.StartCap = LineCap.Round;
			TheGoose.DrawingPen = new Pen(Brushes.White);
			Pen drawingPen = TheGoose.DrawingPen;
			TheGoose.DrawingPen.StartCap = LineCap.Round;
			drawingPen.EndCap = LineCap.Round;
			TheGoose.SetTask(TheGoose.GooseTask.Wander);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003540 File Offset: 0x00001740
		private static void SetSpeed(TheGoose.SpeedTiers tier)
		{
			switch (tier)
			{
			case TheGoose.SpeedTiers.Walk:
				TheGoose.currentSpeed = 80f;
				TheGoose.currentAcceleration = 1300f;
				TheGoose.stepTime = 0.2f;
				return;
			case TheGoose.SpeedTiers.Run:
				TheGoose.currentSpeed = 200f;
				TheGoose.currentAcceleration = 1300f;
				TheGoose.stepTime = 0.2f;
				return;
			case TheGoose.SpeedTiers.Charge:
				TheGoose.currentSpeed = 400f;
				TheGoose.currentAcceleration = 2300f;
				TheGoose.stepTime = 0.1f;
				return;
			default:
				return;
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000035BC File Offset: 0x000017BC
		public static void Tick()
		{
			Cursor.Clip = Rectangle.Empty;
			if (TheGoose.currentTask != TheGoose.GooseTask.NabMouse && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left && !TheGoose.lastFrameMouseButtonPressed && Vector2.Distance(TheGoose.position + new Vector2(0f, 14f), new Vector2((float)Cursor.Position.X, (float)Cursor.Position.Y)) < 30f)
			{
				TheGoose.SetTask(TheGoose.GooseTask.NabMouse);
			}
			TheGoose.lastFrameMouseButtonPressed = ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left);
			TheGoose.targetDirection = Vector2.Normalize(TheGoose.targetPos - TheGoose.position);
			TheGoose.overrideExtendNeck = false;
			TheGoose.RunAI();
			Vector2 vector = Vector2.Lerp(Vector2.GetFromAngleDegrees(TheGoose.direction), TheGoose.targetDirection, 0.25f);
			TheGoose.direction = (float)Math.Atan2((double)vector.y, (double)vector.x) * 57.2957764f;
			if (Vector2.Magnitude(TheGoose.velocity) > TheGoose.currentSpeed)
			{
				TheGoose.velocity = Vector2.Normalize(TheGoose.velocity) * TheGoose.currentSpeed;
			}
			TheGoose.velocity += Vector2.Normalize(TheGoose.targetPos - TheGoose.position) * TheGoose.currentAcceleration * 0.008333334f;
			TheGoose.position += TheGoose.velocity * 0.008333334f;
			TheGoose.SolveFeet();
			Vector2.Magnitude(TheGoose.velocity);
			int num = (TheGoose.overrideExtendNeck | TheGoose.currentSpeed >= 200f) ? 1 : 0;
			TheGoose.gooseRig.neckLerpPercent = SamMath.Lerp(TheGoose.gooseRig.neckLerpPercent, (float)num, 0.075f);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003780 File Offset: 0x00001980
		private static void RunWander()
		{
			if (Time.time - TheGoose.taskWanderInfo.wanderingStartTime > TheGoose.taskWanderInfo.wanderingDuration)
			{
				TheGoose.ChooseNextTask();
				return;
			}
			if (TheGoose.taskWanderInfo.pauseStartTime > 0f)
			{
				if (Time.time - TheGoose.taskWanderInfo.pauseStartTime > TheGoose.taskWanderInfo.pauseDuration)
				{
					TheGoose.taskWanderInfo.pauseStartTime = -1f;
					float num = TheGoose.Task_Wander.GetRandomWalkTime() * TheGoose.currentSpeed;
					TheGoose.targetPos = new Vector2(SamMath.RandomRange(0f, (float)Program.mainForm.Width), SamMath.RandomRange(0f, (float)Program.mainForm.Height));
					if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) > num)
					{
						TheGoose.targetPos = TheGoose.position + Vector2.Normalize(TheGoose.targetPos - TheGoose.position) * num;
					}
					return;
				}
				TheGoose.velocity = Vector2.zero;
				return;
			}
			else
			{
				if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) < 20f)
				{
					TheGoose.taskWanderInfo.pauseStartTime = Time.time;
					TheGoose.taskWanderInfo.pauseDuration = TheGoose.Task_Wander.GetRandomPauseDuration();
					return;
				}
				return;
			}
		}

		// Token: 0x0600004C RID: 76
		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		// Token: 0x0600004D RID: 77 RVA: 0x000038B0 File Offset: 0x00001AB0
		private static void RunNabMouse()
		{
			Vector2 vector = new Vector2((float)Cursor.Position.X, (float)Cursor.Position.Y);
			Vector2 head2EndPoint = TheGoose.gooseRig.head2EndPoint;
			if (TheGoose.taskNabMouseInfo.currentStage == TheGoose.Task_NabMouse.Stage.SeekingMouse)
			{
				TheGoose.SetSpeed(TheGoose.SpeedTiers.Charge);
				TheGoose.targetPos = vector - (TheGoose.gooseRig.head2EndPoint - TheGoose.position);
				if (Vector2.Distance(head2EndPoint, vector) < 15f)
				{
					TheGoose.taskNabMouseInfo.originalVectorToMouse = vector - head2EndPoint;
					TheGoose.taskNabMouseInfo.grabbedOriginalTime = Time.time;
					TheGoose.taskNabMouseInfo.dragToPoint = TheGoose.position;
					while (Vector2.Distance(TheGoose.taskNabMouseInfo.dragToPoint, TheGoose.position) / 400f < 1.2f)
					{
						TheGoose.taskNabMouseInfo.dragToPoint = new Vector2((float)SamMath.Rand.NextDouble() * (float)Program.mainForm.Width, (float)SamMath.Rand.NextDouble() * (float)Program.mainForm.Height);
					}
					TheGoose.targetPos = TheGoose.taskNabMouseInfo.dragToPoint;
					TheGoose.SetForegroundWindow(Program.mainForm.Handle);
					Sound.CHOMP();
					TheGoose.taskNabMouseInfo.currentStage = TheGoose.Task_NabMouse.Stage.DraggingMouseAway;
				}
				if (Time.time > TheGoose.taskNabMouseInfo.chaseStartTime + 9f)
				{
					TheGoose.taskNabMouseInfo.currentStage = TheGoose.Task_NabMouse.Stage.Decelerating;
				}
			}
			if (TheGoose.taskNabMouseInfo.currentStage == TheGoose.Task_NabMouse.Stage.DraggingMouseAway)
			{
				if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) < 30f)
				{
					Cursor.Clip = Rectangle.Empty;
					TheGoose.taskNabMouseInfo.currentStage = TheGoose.Task_NabMouse.Stage.Decelerating;
				}
				else
				{
					float p = Math.Min((Time.time - TheGoose.taskNabMouseInfo.grabbedOriginalTime) / 0.06f, 1f);
					Vector2 vector2 = Vector2.Lerp(TheGoose.taskNabMouseInfo.originalVectorToMouse, TheGoose.Task_NabMouse.StruggleRange, p);
					TheGoose.tmpRect.Location = TheGoose.ToIntPoint(new Vector2
					{
						x = ((vector2.x < 0f) ? (head2EndPoint.x + vector2.x) : head2EndPoint.x),
						y = ((vector2.y < 0f) ? (head2EndPoint.y + vector2.y) : head2EndPoint.y)
					});
					TheGoose.tmpSize.Width = Math.Abs((int)vector2.x);
					TheGoose.tmpSize.Height = Math.Abs((int)vector2.y);
					TheGoose.tmpRect.Size = TheGoose.tmpSize;
					Cursor.Clip = TheGoose.tmpRect;
				}
			}
			if (TheGoose.taskNabMouseInfo.currentStage == TheGoose.Task_NabMouse.Stage.Decelerating)
			{
				TheGoose.targetPos = TheGoose.position + Vector2.Normalize(TheGoose.velocity) * 5f;
				TheGoose.velocity -= Vector2.Normalize(TheGoose.velocity) * TheGoose.currentAcceleration * 2f * 0.008333334f;
				if (Vector2.Magnitude(TheGoose.velocity) < 80f)
				{
					TheGoose.SetTask(TheGoose.GooseTask.Wander);
				}
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003BBC File Offset: 0x00001DBC
		private static void RunCollectWindow()
		{
			switch (TheGoose.taskCollectWindowInfo.stage)
			{
			case TheGoose.Task_CollectWindow.Stage.WalkingOffscreen:
				if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) < 5f)
				{
					TheGoose.taskCollectWindowInfo.secsToWait = TheGoose.Task_CollectWindow.GetWaitTime();
					TheGoose.taskCollectWindowInfo.waitStartTime = Time.time;
					TheGoose.taskCollectWindowInfo.stage = TheGoose.Task_CollectWindow.Stage.WaitingToBringWindowBack;
					return;
				}
				break;
			case TheGoose.Task_CollectWindow.Stage.WaitingToBringWindowBack:
				if (Time.time - TheGoose.taskCollectWindowInfo.waitStartTime > TheGoose.taskCollectWindowInfo.secsToWait)
				{
					TheGoose.taskCollectWindowInfo.mainForm.FormClosing += TheGoose.CollectMemeTask_CancelEarly;
					new Thread(delegate()
					{
						TheGoose.taskCollectWindowInfo.mainForm.ShowDialog();
					}).Start();
					switch (TheGoose.taskCollectWindowInfo.screenDirection)
					{
					case TheGoose.Task_CollectWindow.ScreenDirection.Left:
						TheGoose.targetPos.y = SamMath.Lerp(TheGoose.position.y, (float)(Program.mainForm.Height / 2), SamMath.RandomRange(0.2f, 0.3f));
						TheGoose.targetPos.x = (float)TheGoose.taskCollectWindowInfo.mainForm.Width + SamMath.RandomRange(15f, 20f);
						break;
					case TheGoose.Task_CollectWindow.ScreenDirection.Top:
						TheGoose.targetPos.y = (float)TheGoose.taskCollectWindowInfo.mainForm.Height + SamMath.RandomRange(80f, 100f);
						TheGoose.targetPos.x = SamMath.Lerp(TheGoose.position.x, (float)(Program.mainForm.Width / 2), SamMath.RandomRange(0.2f, 0.3f));
						break;
					case TheGoose.Task_CollectWindow.ScreenDirection.Right:
						TheGoose.targetPos.y = SamMath.Lerp(TheGoose.position.y, (float)(Program.mainForm.Height / 2), SamMath.RandomRange(0.2f, 0.3f));
						TheGoose.targetPos.x = (float)Program.mainForm.Width - ((float)TheGoose.taskCollectWindowInfo.mainForm.Width + SamMath.RandomRange(20f, 30f));
						break;
					}
					TheGoose.targetPos.x = SamMath.Clamp(TheGoose.targetPos.x, (float)(TheGoose.taskCollectWindowInfo.mainForm.Width + 55), (float)(Program.mainForm.Width - (TheGoose.taskCollectWindowInfo.mainForm.Width + 55)));
					TheGoose.targetPos.y = SamMath.Clamp(TheGoose.targetPos.y, (float)(TheGoose.taskCollectWindowInfo.mainForm.Height + 80), (float)Program.mainForm.Height);
					TheGoose.taskCollectWindowInfo.stage = TheGoose.Task_CollectWindow.Stage.DraggingWindowBack;
					return;
				}
				break;
			case TheGoose.Task_CollectWindow.Stage.DraggingWindowBack:
				if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) < 5f)
				{
					TheGoose.targetPos = TheGoose.position + Vector2.GetFromAngleDegrees(TheGoose.direction + 180f) * 40f;
					TheGoose.SetTask(TheGoose.GooseTask.Wander);
					return;
				}
				TheGoose.overrideExtendNeck = true;
				TheGoose.targetDirection = TheGoose.position - TheGoose.targetPos;
				TheGoose.taskCollectWindowInfo.mainForm.SetWindowPositionThreadsafe(TheGoose.ToIntPoint(TheGoose.gooseRig.head2EndPoint - TheGoose.taskCollectWindowInfo.windowOffsetToBeak));
				break;
			default:
				return;
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x0000252A File Offset: 0x0000072A
		private static void CollectMemeTask_CancelEarly(object sender, FormClosingEventArgs e)
		{
			TheGoose.SetTask(TheGoose.GooseTask.NabMouse);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003EFC File Offset: 0x000020FC
		private static void RunTrackMud()
		{
			switch (TheGoose.taskTrackMudInfo.stage)
			{
			case TheGoose.Task_TrackMud.Stage.DecideToRun:
				TheGoose.SetTargetOffscreen(false);
				TheGoose.SetSpeed(TheGoose.SpeedTiers.Run);
				TheGoose.taskTrackMudInfo.stage = TheGoose.Task_TrackMud.Stage.RunningOffscreen;
				return;
			case TheGoose.Task_TrackMud.Stage.RunningOffscreen:
				if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) < 5f)
				{
					TheGoose.targetPos = new Vector2(SamMath.RandomRange(0f, (float)Program.mainForm.Width), SamMath.RandomRange(0f, (float)Program.mainForm.Height));
					TheGoose.taskTrackMudInfo.nextDirChangeTime = Time.time + TheGoose.Task_TrackMud.GetDirChangeInterval();
					TheGoose.taskTrackMudInfo.timeToStopRunning = Time.time + 2f;
					TheGoose.trackMudEndTime = Time.time + 15f;
					TheGoose.taskTrackMudInfo.stage = TheGoose.Task_TrackMud.Stage.RunningWandering;
					Sound.PlayMudSquith();
					return;
				}
				break;
			case TheGoose.Task_TrackMud.Stage.RunningWandering:
				if (Vector2.Distance(TheGoose.position, TheGoose.targetPos) < 5f || Time.time > TheGoose.taskTrackMudInfo.nextDirChangeTime)
				{
					TheGoose.targetPos = new Vector2(SamMath.RandomRange(0f, (float)Program.mainForm.Width), SamMath.RandomRange(0f, (float)Program.mainForm.Height));
					TheGoose.taskTrackMudInfo.nextDirChangeTime = Time.time + TheGoose.Task_TrackMud.GetDirChangeInterval();
				}
				if (Time.time > TheGoose.taskTrackMudInfo.timeToStopRunning)
				{
					TheGoose.targetPos = TheGoose.position + new Vector2(30f, 3f);
					TheGoose.targetPos.x = SamMath.Clamp(TheGoose.targetPos.x, 55f, (float)(Program.mainForm.Width - 55));
					TheGoose.targetPos.y = SamMath.Clamp(TheGoose.targetPos.y, 80f, (float)(Program.mainForm.Height - 80));
					TheGoose.SetTask(TheGoose.GooseTask.Wander, false);
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000040D8 File Offset: 0x000022D8
		private static void ChooseNextTask()
		{
			if (!GooseConfig.settings.CanAttackAtRandom && Time.time < GooseConfig.settings.FirstWanderTimeSeconds + 1f)
			{
				TheGoose.SetTask(TheGoose.GooseTask.TrackMud);
				return;
			}
			if (Time.time > 480f && !TheGoose.hasAskedForDonation)
			{
				TheGoose.hasAskedForDonation = true;
				TheGoose.SetTask(TheGoose.GooseTask.CollectWindow_Donate);
				return;
			}
			TheGoose.GooseTask gooseTask = TheGoose.gooseTaskWeightedList[TheGoose.taskPickerDeck.Next()];
			while (!GooseConfig.settings.CanAttackAtRandom)
			{
				if (gooseTask != TheGoose.GooseTask.NabMouse)
				{
					break;
				}
				gooseTask = TheGoose.gooseTaskWeightedList[TheGoose.taskPickerDeck.Next()];
			}
			TheGoose.SetTask(gooseTask);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002532 File Offset: 0x00000732
		private static void SetTask(TheGoose.GooseTask task)
		{
			TheGoose.SetTask(task, true);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x0000416C File Offset: 0x0000236C
		private static void SetTask(TheGoose.GooseTask task, bool honck)
		{
			if (honck)
			{
				Sound.HONCC();
			}
			TheGoose.currentTask = task;
			switch (task)
			{
			case TheGoose.GooseTask.Wander:
				TheGoose.SetSpeed(TheGoose.SpeedTiers.Walk);
				TheGoose.taskWanderInfo = default(TheGoose.Task_Wander);
				TheGoose.taskWanderInfo.pauseStartTime = -1f;
				TheGoose.taskWanderInfo.wanderingStartTime = Time.time;
				TheGoose.taskWanderInfo.wanderingDuration = TheGoose.Task_Wander.GetRandomWanderDuration();
				return;
			case TheGoose.GooseTask.NabMouse:
				TheGoose.taskNabMouseInfo = default(TheGoose.Task_NabMouse);
				TheGoose.taskNabMouseInfo.chaseStartTime = Time.time;
				return;
			case TheGoose.GooseTask.CollectWindow_Meme:
				TheGoose.taskCollectWindowInfo = default(TheGoose.Task_CollectWindow);
				TheGoose.taskCollectWindowInfo.mainForm = new TheGoose.SimpleImageForm();
				TheGoose.SetTask(TheGoose.GooseTask.CollectWindow_DONOTSET, false);
				return;
			case TheGoose.GooseTask.CollectWindow_Notepad:
				TheGoose.taskCollectWindowInfo = default(TheGoose.Task_CollectWindow);
				TheGoose.taskCollectWindowInfo.mainForm = new TheGoose.SimpleTextForm();
				TheGoose.SetTask(TheGoose.GooseTask.CollectWindow_DONOTSET, false);
				return;
			case TheGoose.GooseTask.CollectWindow_Donate:
				TheGoose.taskCollectWindowInfo = default(TheGoose.Task_CollectWindow);
				TheGoose.taskCollectWindowInfo.mainForm = new TheGoose.SimpleDonateForm();
				TheGoose.SetTask(TheGoose.GooseTask.CollectWindow_DONOTSET, false);
				return;
			case TheGoose.GooseTask.CollectWindow_DONOTSET:
				TheGoose.taskCollectWindowInfo.screenDirection = TheGoose.SetTargetOffscreen(false);
				switch (TheGoose.taskCollectWindowInfo.screenDirection)
				{
				case TheGoose.Task_CollectWindow.ScreenDirection.Left:
					TheGoose.taskCollectWindowInfo.windowOffsetToBeak = new Vector2((float)TheGoose.taskCollectWindowInfo.mainForm.Width, (float)(TheGoose.taskCollectWindowInfo.mainForm.Height / 2));
					return;
				case TheGoose.Task_CollectWindow.ScreenDirection.Top:
					TheGoose.taskCollectWindowInfo.windowOffsetToBeak = new Vector2((float)(TheGoose.taskCollectWindowInfo.mainForm.Width / 2), (float)TheGoose.taskCollectWindowInfo.mainForm.Height);
					return;
				case TheGoose.Task_CollectWindow.ScreenDirection.Right:
					TheGoose.taskCollectWindowInfo.windowOffsetToBeak = new Vector2(0f, (float)(TheGoose.taskCollectWindowInfo.mainForm.Height / 2));
					return;
				default:
					return;
				}
				break;
			case TheGoose.GooseTask.TrackMud:
				TheGoose.taskTrackMudInfo = default(TheGoose.Task_TrackMud);
				return;
			default:
				return;
			}
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004330 File Offset: 0x00002530
		private static void RunAI()
		{
			switch (TheGoose.currentTask)
			{
			case TheGoose.GooseTask.Wander:
				TheGoose.RunWander();
				return;
			case TheGoose.GooseTask.NabMouse:
				TheGoose.RunNabMouse();
				return;
			case TheGoose.GooseTask.CollectWindow_Meme:
			case TheGoose.GooseTask.CollectWindow_Notepad:
			case TheGoose.GooseTask.CollectWindow_Donate:
				break;
			case TheGoose.GooseTask.CollectWindow_DONOTSET:
				TheGoose.RunCollectWindow();
				return;
			case TheGoose.GooseTask.TrackMud:
				TheGoose.RunTrackMud();
				break;
			default:
				return;
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00004380 File Offset: 0x00002580
		private static TheGoose.Task_CollectWindow.ScreenDirection SetTargetOffscreen(bool canExitTop = false)
		{
			int num = (int)TheGoose.position.x;
			TheGoose.Task_CollectWindow.ScreenDirection result = TheGoose.Task_CollectWindow.ScreenDirection.Left;
			TheGoose.targetPos = new Vector2(-50f, SamMath.Lerp(TheGoose.position.y, (float)(Program.mainForm.Height / 2), 0.4f));
			if (num > Program.mainForm.Width / 2)
			{
				num = Program.mainForm.Width - (int)TheGoose.position.x;
				result = TheGoose.Task_CollectWindow.ScreenDirection.Right;
				TheGoose.targetPos = new Vector2((float)(Program.mainForm.Width + 50), SamMath.Lerp(TheGoose.position.y, (float)(Program.mainForm.Height / 2), 0.4f));
			}
			if (canExitTop && (float)num > TheGoose.position.y)
			{
				result = TheGoose.Task_CollectWindow.ScreenDirection.Top;
				TheGoose.targetPos = new Vector2(SamMath.Lerp(TheGoose.position.x, (float)(Program.mainForm.Width / 2), 0.4f), -50f);
			}
			return result;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00004470 File Offset: 0x00002670
		private static void SolveFeet()
		{
			Vector2.GetFromAngleDegrees(TheGoose.direction);
			Vector2.GetFromAngleDegrees(TheGoose.direction + 90f);
			Vector2 footHome = TheGoose.GetFootHome(false);
			Vector2 footHome2 = TheGoose.GetFootHome(true);
			if (TheGoose.lFootMoveTimeStart < 0f && TheGoose.rFootMoveTimeStart < 0f)
			{
				if (Vector2.Distance(TheGoose.lFootPos, footHome) > 5f)
				{
					TheGoose.lFootMoveOrigin = TheGoose.lFootPos;
					TheGoose.lFootMoveDir = Vector2.Normalize(footHome - TheGoose.lFootPos);
					TheGoose.lFootMoveTimeStart = Time.time;
					return;
				}
				if (Vector2.Distance(TheGoose.rFootPos, footHome2) > 5f)
				{
					TheGoose.rFootMoveOrigin = TheGoose.rFootPos;
					TheGoose.rFootMoveDir = Vector2.Normalize(footHome2 - TheGoose.rFootPos);
					TheGoose.rFootMoveTimeStart = Time.time;
					return;
				}
			}
			else if (TheGoose.lFootMoveTimeStart > 0f)
			{
				Vector2 b = footHome + TheGoose.lFootMoveDir * 0.4f * 5f;
				if (Time.time <= TheGoose.lFootMoveTimeStart + TheGoose.stepTime)
				{
					float p = (Time.time - TheGoose.lFootMoveTimeStart) / TheGoose.stepTime;
					TheGoose.lFootPos = Vector2.Lerp(TheGoose.lFootMoveOrigin, b, Easings.CubicEaseInOut(p));
					return;
				}
				TheGoose.lFootPos = b;
				TheGoose.lFootMoveTimeStart = -1f;
				Sound.PlayPat();
				if (Time.time < TheGoose.trackMudEndTime)
				{
					TheGoose.AddFootMark(TheGoose.lFootPos);
					return;
				}
			}
			else if (TheGoose.rFootMoveTimeStart > 0f)
			{
				Vector2 b2 = footHome2 + TheGoose.rFootMoveDir * 0.4f * 5f;
				if (Time.time > TheGoose.rFootMoveTimeStart + TheGoose.stepTime)
				{
					TheGoose.rFootPos = b2;
					TheGoose.rFootMoveTimeStart = -1f;
					Sound.PlayPat();
					if (Time.time < TheGoose.trackMudEndTime)
					{
						TheGoose.AddFootMark(TheGoose.rFootPos);
						return;
					}
				}
				else
				{
					float p2 = (Time.time - TheGoose.rFootMoveTimeStart) / TheGoose.stepTime;
					TheGoose.rFootPos = Vector2.Lerp(TheGoose.rFootMoveOrigin, b2, Easings.CubicEaseInOut(p2));
				}
			}
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00004670 File Offset: 0x00002870
		private static Vector2 GetFootHome(bool rightFoot)
		{
			float b = (float)(rightFoot ? 1 : 0);
			Vector2 a = Vector2.GetFromAngleDegrees(TheGoose.direction + 90f) * b;
			return TheGoose.position + a * 6f;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x000046B4 File Offset: 0x000028B4
		private static void AddFootMark(Vector2 markPos)
		{
			TheGoose.footMarks[TheGoose.footMarkIndex].time = Time.time;
			TheGoose.footMarks[TheGoose.footMarkIndex].position = markPos;
			TheGoose.footMarkIndex++;
			if (TheGoose.footMarkIndex >= TheGoose.footMarks.Length)
			{
				TheGoose.footMarkIndex = 0;
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00004710 File Offset: 0x00002910
		public static void UpdateRig()
		{
			float num = TheGoose.direction;
			int num2 = (int)TheGoose.position.x;
			int num3 = (int)TheGoose.position.y;
			Vector2 a = new Vector2((float)num2, (float)num3);
			Vector2 b = new Vector2(1.3f, 0.4f);
			Vector2 fromAngleDegrees = Vector2.GetFromAngleDegrees(num);
			fromAngleDegrees * b;
			Vector2.GetFromAngleDegrees(num + 90f) * b;
			Vector2 a2 = new Vector2(0f, -1f);
			TheGoose.gooseRig.underbodyCenter = a + a2 * 9f;
			TheGoose.gooseRig.bodyCenter = a + a2 * 14f;
			int num4 = (int)SamMath.Lerp(20f, 10f, TheGoose.gooseRig.neckLerpPercent);
			int num5 = (int)SamMath.Lerp(3f, 16f, TheGoose.gooseRig.neckLerpPercent);
			TheGoose.gooseRig.neckCenter = a + a2 * (float)(14 + num4);
			TheGoose.gooseRig.neckBase = TheGoose.gooseRig.bodyCenter + fromAngleDegrees * 15f;
			TheGoose.gooseRig.neckHeadPoint = TheGoose.gooseRig.neckBase + fromAngleDegrees * (float)num5 + a2 * (float)num4;
			TheGoose.gooseRig.head1EndPoint = TheGoose.gooseRig.neckHeadPoint + fromAngleDegrees * 3f - a2 * 1f;
			TheGoose.gooseRig.head2EndPoint = TheGoose.gooseRig.head1EndPoint + fromAngleDegrees * 5f;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000048CC File Offset: 0x00002ACC
		public static void Render(Graphics g)
		{
			for (int i = 0; i < TheGoose.footMarks.Length; i++)
			{
				if (TheGoose.footMarks[i].time != 0f)
				{
					float num = TheGoose.footMarks[i].time + 8.5f;
					float p = SamMath.Clamp(Time.time - num, 0f, 1f) / 1f;
					float num2 = SamMath.Lerp(3f, 0f, p);
					TheGoose.FillCircleFromCenter(g, Brushes.SaddleBrown, TheGoose.footMarks[i].position, (int)num2);
				}
			}
			TheGoose.UpdateRig();
			float num3 = TheGoose.direction;
			int num4 = (int)TheGoose.position.x;
			int num5 = (int)TheGoose.position.y;
			Vector2 vector = new Vector2((float)num4, (float)num5);
			Vector2 b = new Vector2(1.3f, 0.4f);
			Vector2 fromAngleDegrees = Vector2.GetFromAngleDegrees(num3);
			fromAngleDegrees * b;
			Vector2 fromAngleDegrees2 = Vector2.GetFromAngleDegrees(num3 + 90f);
			fromAngleDegrees2 * b;
			Vector2 a = new Vector2(0f, -1f);
			TheGoose.DrawingPen.Brush = Brushes.White;
			TheGoose.FillCircleFromCenter(g, Brushes.Orange, TheGoose.lFootPos, 4);
			TheGoose.FillCircleFromCenter(g, Brushes.Orange, TheGoose.rFootPos, 4);
			TheGoose.FillEllipseFromCenter(g, TheGoose.shadowBrush, (int)vector.x, (int)vector.y, 20, 15);
			TheGoose.DrawingPen.Color = Color.LightGray;
			TheGoose.DrawingPen.Width = 24f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.bodyCenter + fromAngleDegrees * 11f), TheGoose.ToIntPoint(TheGoose.gooseRig.bodyCenter - fromAngleDegrees * 11f));
			TheGoose.DrawingPen.Width = 15f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.neckBase), TheGoose.ToIntPoint(TheGoose.gooseRig.neckHeadPoint));
			TheGoose.DrawingPen.Width = 17f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.neckHeadPoint), TheGoose.ToIntPoint(TheGoose.gooseRig.head1EndPoint));
			TheGoose.DrawingPen.Width = 12f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.head1EndPoint), TheGoose.ToIntPoint(TheGoose.gooseRig.head2EndPoint));
			TheGoose.DrawingPen.Color = Color.LightGray;
			TheGoose.DrawingPen.Width = 15f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.underbodyCenter + fromAngleDegrees * 7f), TheGoose.ToIntPoint(TheGoose.gooseRig.underbodyCenter - fromAngleDegrees * 7f));
			TheGoose.DrawingPen.Color = Color.White;
			TheGoose.DrawingPen.Width = 22f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.bodyCenter + fromAngleDegrees * 11f), TheGoose.ToIntPoint(TheGoose.gooseRig.bodyCenter - fromAngleDegrees * 11f));
			TheGoose.DrawingPen.Width = 13f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.neckBase), TheGoose.ToIntPoint(TheGoose.gooseRig.neckHeadPoint));
			TheGoose.DrawingPen.Width = 15f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.neckHeadPoint), TheGoose.ToIntPoint(TheGoose.gooseRig.head1EndPoint));
			TheGoose.DrawingPen.Width = 10f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.head1EndPoint), TheGoose.ToIntPoint(TheGoose.gooseRig.head2EndPoint));
			TheGoose.DrawingPen.Width = 9f;
			TheGoose.DrawingPen.Brush = Brushes.Orange;
			Vector2 vector2 = TheGoose.gooseRig.head2EndPoint + fromAngleDegrees * 3f;
			g.DrawLine(TheGoose.DrawingPen, TheGoose.ToIntPoint(TheGoose.gooseRig.head2EndPoint), TheGoose.ToIntPoint(vector2));
			Vector2 pos = TheGoose.gooseRig.neckHeadPoint + a * 3f + -fromAngleDegrees2 * b * 5f + fromAngleDegrees * 5f;
			Vector2 pos2 = TheGoose.gooseRig.neckHeadPoint + a * 3f + fromAngleDegrees2 * b * 5f + fromAngleDegrees * 5f;
			TheGoose.FillCircleFromCenter(g, Brushes.Black, pos, 2);
			TheGoose.FillCircleFromCenter(g, Brushes.Black, pos2, 2);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000253B File Offset: 0x0000073B
		public static void FillCircleFromCenter(Graphics g, Brush brush, Vector2 pos, int radius)
		{
			TheGoose.FillEllipseFromCenter(g, brush, (int)pos.x, (int)pos.y, radius, radius);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00002554 File Offset: 0x00000754
		public static void FillCircleFromCenter(Graphics g, Brush brush, int x, int y, int radius)
		{
			TheGoose.FillEllipseFromCenter(g, brush, x, y, radius, radius);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00002563 File Offset: 0x00000763
		public static void FillEllipseFromCenter(Graphics g, Brush brush, int x, int y, int xRadius, int yRadius)
		{
			g.FillEllipse(brush, x - xRadius, y - yRadius, xRadius * 2, yRadius * 2);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x0000257C File Offset: 0x0000077C
		public static void FillEllipseFromCenter(Graphics g, Brush brush, Vector2 position, Vector2 xyRadius)
		{
			g.FillEllipse(brush, position.x - xyRadius.x, position.y - xyRadius.y, xyRadius.x * 2f, xyRadius.y * 2f);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000025B7 File Offset: 0x000007B7
		private static Point ToIntPoint(Vector2 vector)
		{
			return new Point((int)vector.x, (int)vector.y);
		}

		// Token: 0x0400001C RID: 28
		private static Vector2 position = new Vector2(300f, 300f);

		// Token: 0x0400001D RID: 29
		private static Vector2 velocity = new Vector2(0f, 0f);

		// Token: 0x0400001E RID: 30
		private static float direction = 90f;

		// Token: 0x0400001F RID: 31
		private static Vector2 targetDirection;

		// Token: 0x04000020 RID: 32
		private static bool overrideExtendNeck;

		// Token: 0x04000021 RID: 33
		private const TheGoose.GooseTask FirstUX_FirstTask = TheGoose.GooseTask.TrackMud;

		// Token: 0x04000022 RID: 34
		private const TheGoose.GooseTask FirstUX_SecondTask = TheGoose.GooseTask.CollectWindow_Meme;

		// Token: 0x04000023 RID: 35
		private static Vector2 targetPos = new Vector2(300f, 300f);

		// Token: 0x04000024 RID: 36
		private static float targetDir = 90f;

		// Token: 0x04000025 RID: 37
		private static float currentSpeed = 80f;

		// Token: 0x04000026 RID: 38
		private static float currentAcceleration = 1300f;

		// Token: 0x04000027 RID: 39
		private static float stepTime = 0.2f;

		// Token: 0x04000028 RID: 40
		private const float WalkSpeed = 80f;

		// Token: 0x04000029 RID: 41
		private const float RunSpeed = 200f;

		// Token: 0x0400002A RID: 42
		private const float ChargeSpeed = 400f;

		// Token: 0x0400002B RID: 43
		private const float turnSpeed = 120f;

		// Token: 0x0400002C RID: 44
		private const float AccelerationNormal = 1300f;

		// Token: 0x0400002D RID: 45
		private const float AccelerationCharged = 2300f;

		// Token: 0x0400002E RID: 46
		private const float StopRadius = -10f;

		// Token: 0x0400002F RID: 47
		private const float StepTimeNormal = 0.2f;

		// Token: 0x04000030 RID: 48
		private const float StepTimeCharged = 0.1f;

		// Token: 0x04000031 RID: 49
		private static float trackMudEndTime = -1f;

		// Token: 0x04000032 RID: 50
		private const float DurationToTrackMud = 15f;

		// Token: 0x04000033 RID: 51
		private static Pen DrawingPen;

		// Token: 0x04000034 RID: 52
		private static Bitmap shadowBitmap;

		// Token: 0x04000035 RID: 53
		private static TextureBrush shadowBrush;

		// Token: 0x04000036 RID: 54
		private static Pen shadowPen;

		// Token: 0x04000037 RID: 55
		private static FootMark[] footMarks = new FootMark[64];

		// Token: 0x04000038 RID: 56
		private static int footMarkIndex = 0;

		// Token: 0x04000039 RID: 57
		private static bool lastFrameMouseButtonPressed = false;

		// Token: 0x0400003A RID: 58
		private static TheGoose.GooseTask currentTask;

		// Token: 0x0400003B RID: 59
		private static TheGoose.Task_Wander taskWanderInfo;

		// Token: 0x0400003C RID: 60
		private static TheGoose.Task_NabMouse taskNabMouseInfo;

		// Token: 0x0400003D RID: 61
		private static Rectangle tmpRect = default(Rectangle);

		// Token: 0x0400003E RID: 62
		private static Size tmpSize = default(Size);

		// Token: 0x0400003F RID: 63
		private static bool hasAskedForDonation = false;

		// Token: 0x04000040 RID: 64
		private static TheGoose.Task_CollectWindow taskCollectWindowInfo;

		// Token: 0x04000041 RID: 65
		private static TheGoose.Task_TrackMud taskTrackMudInfo;

		// Token: 0x04000042 RID: 66
		private static TheGoose.GooseTask[] gooseTaskWeightedList = new TheGoose.GooseTask[]
		{
			TheGoose.GooseTask.TrackMud,
			TheGoose.GooseTask.TrackMud,
			TheGoose.GooseTask.CollectWindow_Meme,
			TheGoose.GooseTask.CollectWindow_Meme,
			TheGoose.GooseTask.CollectWindow_Notepad,
			TheGoose.GooseTask.NabMouse,
			TheGoose.GooseTask.NabMouse,
			TheGoose.GooseTask.NabMouse
		};

		// Token: 0x04000043 RID: 67
		private static Deck taskPickerDeck = new Deck(TheGoose.gooseTaskWeightedList.Length);

		// Token: 0x04000044 RID: 68
		private static Vector2 lFootPos;

		// Token: 0x04000045 RID: 69
		private static Vector2 rFootPos;

		// Token: 0x04000046 RID: 70
		private static float lFootMoveTimeStart = -1f;

		// Token: 0x04000047 RID: 71
		private static float rFootMoveTimeStart = -1f;

		// Token: 0x04000048 RID: 72
		private static Vector2 lFootMoveOrigin;

		// Token: 0x04000049 RID: 73
		private static Vector2 rFootMoveOrigin;

		// Token: 0x0400004A RID: 74
		private static Vector2 lFootMoveDir;

		// Token: 0x0400004B RID: 75
		private static Vector2 rFootMoveDir;

		// Token: 0x0400004C RID: 76
		private const float wantStepAtDistance = 5f;

		// Token: 0x0400004D RID: 77
		private const int feetDistanceApart = 6;

		// Token: 0x0400004E RID: 78
		private const float overshootFraction = 0.4f;

		// Token: 0x0400004F RID: 79
		private static TheGoose.Rig gooseRig;

		// Token: 0x02000017 RID: 23
		private enum SpeedTiers
		{
			// Token: 0x0400008E RID: 142
			Walk,
			// Token: 0x0400008F RID: 143
			Run,
			// Token: 0x04000090 RID: 144
			Charge
		}

		// Token: 0x02000018 RID: 24
		private enum GooseTask
		{
			// Token: 0x04000092 RID: 146
			Wander,
			// Token: 0x04000093 RID: 147
			NabMouse,
			// Token: 0x04000094 RID: 148
			CollectWindow_Meme,
			// Token: 0x04000095 RID: 149
			CollectWindow_Notepad,
			// Token: 0x04000096 RID: 150
			CollectWindow_Donate,
			// Token: 0x04000097 RID: 151
			CollectWindow_DONOTSET,
			// Token: 0x04000098 RID: 152
			TrackMud,
			// Token: 0x04000099 RID: 153
			Count
		}

		// Token: 0x02000019 RID: 25
		private struct Task_Wander
		{
			// Token: 0x06000085 RID: 133 RVA: 0x00002803 File Offset: 0x00000A03
			public static float GetRandomPauseDuration()
			{
				return 1f + (float)SamMath.Rand.NextDouble() * 1f;
			}

			// Token: 0x06000086 RID: 134 RVA: 0x0000281C File Offset: 0x00000A1C
			public static float GetRandomWanderDuration()
			{
				if (Time.time < 1f)
				{
					return GooseConfig.settings.FirstWanderTimeSeconds;
				}
				return SamMath.RandomRange(GooseConfig.settings.MinWanderingTimeSeconds, GooseConfig.settings.MaxWanderingTimeSeconds);
			}

			// Token: 0x06000087 RID: 135 RVA: 0x0000284E File Offset: 0x00000A4E
			public static float GetRandomWalkTime()
			{
				return SamMath.RandomRange(1f, 6f);
			}

			// Token: 0x0400009A RID: 154
			private const float MinPauseTime = 1f;

			// Token: 0x0400009B RID: 155
			private const float MaxPauseTime = 2f;

			// Token: 0x0400009C RID: 156
			public const float GoodEnoughDistance = 20f;

			// Token: 0x0400009D RID: 157
			public float wanderingStartTime;

			// Token: 0x0400009E RID: 158
			public float wanderingDuration;

			// Token: 0x0400009F RID: 159
			public float pauseStartTime;

			// Token: 0x040000A0 RID: 160
			public float pauseDuration;
		}

		// Token: 0x0200001A RID: 26
		private struct Task_NabMouse
		{
			// Token: 0x040000A1 RID: 161
			public TheGoose.Task_NabMouse.Stage currentStage;

			// Token: 0x040000A2 RID: 162
			public Vector2 dragToPoint;

			// Token: 0x040000A3 RID: 163
			public float grabbedOriginalTime;

			// Token: 0x040000A4 RID: 164
			public float chaseStartTime;

			// Token: 0x040000A5 RID: 165
			public Vector2 originalVectorToMouse;

			// Token: 0x040000A6 RID: 166
			public const float MouseGrabDistance = 15f;

			// Token: 0x040000A7 RID: 167
			public const float MouseSuccTime = 0.06f;

			// Token: 0x040000A8 RID: 168
			public const float MouseDropDistance = 30f;

			// Token: 0x040000A9 RID: 169
			public const float MinRunTime = 2f;

			// Token: 0x040000AA RID: 170
			public const float MaxRunTime = 4f;

			// Token: 0x040000AB RID: 171
			public const float GiveUpTime = 9f;

			// Token: 0x040000AC RID: 172
			public static readonly Vector2 StruggleRange = new Vector2(3f, 3f);

			// Token: 0x02000025 RID: 37
			public enum Stage
			{
				// Token: 0x040000E8 RID: 232
				SeekingMouse,
				// Token: 0x040000E9 RID: 233
				DraggingMouseAway,
				// Token: 0x040000EA RID: 234
				Decelerating
			}
		}

		// Token: 0x0200001B RID: 27
		private struct Task_CollectWindow
		{
			// Token: 0x06000089 RID: 137 RVA: 0x00002875 File Offset: 0x00000A75
			public static float GetWaitTime()
			{
				return SamMath.RandomRange(2f, 3.5f);
			}

			// Token: 0x040000AD RID: 173
			public TheGoose.MovableForm mainForm;

			// Token: 0x040000AE RID: 174
			public TheGoose.Task_CollectWindow.Stage stage;

			// Token: 0x040000AF RID: 175
			public float secsToWait;

			// Token: 0x040000B0 RID: 176
			public float waitStartTime;

			// Token: 0x040000B1 RID: 177
			public TheGoose.Task_CollectWindow.ScreenDirection screenDirection;

			// Token: 0x040000B2 RID: 178
			public Vector2 windowOffsetToBeak;

			// Token: 0x02000026 RID: 38
			public enum Stage
			{
				// Token: 0x040000EC RID: 236
				WalkingOffscreen,
				// Token: 0x040000ED RID: 237
				WaitingToBringWindowBack,
				// Token: 0x040000EE RID: 238
				DraggingWindowBack
			}

			// Token: 0x02000027 RID: 39
			public enum ScreenDirection
			{
				// Token: 0x040000F0 RID: 240
				Left,
				// Token: 0x040000F1 RID: 241
				Top,
				// Token: 0x040000F2 RID: 242
				Right
			}
		}

		// Token: 0x0200001C RID: 28
		private class MovableForm : Form
		{
			// Token: 0x0600008A RID: 138 RVA: 0x0000534C File Offset: 0x0000354C
			public MovableForm()
			{
				base.StartPosition = FormStartPosition.Manual;
				base.Width = 400;
				base.Height = 400;
				this.BackColor = Color.DimGray;
				base.Icon = null;
				base.ShowIcon = false;
				this.SetWindowResizableThreadsafe(false);
			}

			// Token: 0x0600008B RID: 139 RVA: 0x0000539C File Offset: 0x0000359C
			public void SetWindowPositionThreadsafe(Point p)
			{
				if (base.InvokeRequired)
				{
					base.BeginInvoke(new MethodInvoker(delegate()
					{
						this.Location = p;
						this.TopMost = true;
					}));
					return;
				}
				base.Location = p;
				base.TopMost = true;
			}

			// Token: 0x0600008C RID: 140 RVA: 0x000053EC File Offset: 0x000035EC
			public void SetWindowResizableThreadsafe(bool canResize)
			{
				if (base.InvokeRequired)
				{
					base.BeginInvoke(new MethodInvoker(delegate()
					{
						this.FormBorderStyle = (canResize ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle);
						this.MaximizeBox = (this.MinimizeBox = canResize);
					}));
					return;
				}
				base.FormBorderStyle = (canResize ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle);
				base.MaximizeBox = (base.MinimizeBox = canResize);
			}
		}

		// Token: 0x0200001D RID: 29
		private class SimpleImageForm : TheGoose.MovableForm
		{
			// Token: 0x0600008D RID: 141 RVA: 0x00005450 File Offset: 0x00003650
			public SimpleImageForm()
			{
				List<Image> list = new List<Image>();
				try
				{
					string[] files = Directory.GetFiles(TheGoose.SimpleImageForm.memesRootFolder);
					for (int i = 0; i < files.Length; i++)
					{
						Image image = Image.FromFile(files[i]);
						if (image != null)
						{
							list.Add(image);
						}
					}
				}
				catch
				{
				}
				this.localImages = list.ToArray();
				this.localImageDeck = new Deck(this.localImages.Length);
				PictureBox pictureBox = new PictureBox();
				pictureBox.Dock = DockStyle.Fill;
				try
				{
					pictureBox.Image = this.localImages[this.localImageDeck.Next()];
				}
				catch
				{
					pictureBox.LoadAsync(TheGoose.SimpleImageForm.imageURLs[TheGoose.SimpleImageForm.imageURLDeck.Next()]);
				}
				pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
				base.Controls.Add(pictureBox);
			}

			// Token: 0x040000B3 RID: 179
			private static readonly string memesRootFolder = Program.GetPathToFileInAssembly("Assets/Images/Memes/");

			// Token: 0x040000B4 RID: 180
			private Image[] localImages;

			// Token: 0x040000B5 RID: 181
			private Deck localImageDeck;

			// Token: 0x040000B6 RID: 182
			private static string[] imageURLs = new string[]
			{
				"https://preview.redd.it/dsfjv8aev0p31.png?width=960&crop=smart&auto=webp&s=1d58948acc5c6dd60df1092c1bd2a59a509069fd",
				"https://i.redd.it/4ojv59zvglp31.jpg",
				"https://i.redd.it/4bamd6lnso241.jpg",
				"https://i.redd.it/5i5et9p1vsp31.jpg",
				"https://i.redd.it/j2f1i9djx5p31.jpg"
			};

			// Token: 0x040000B7 RID: 183
			private static Deck imageURLDeck = new Deck(TheGoose.SimpleImageForm.imageURLs.Length);
		}

		// Token: 0x0200001E RID: 30
		private class SimpleTextForm : TheGoose.MovableForm
		{
			// Token: 0x0600008F RID: 143 RVA: 0x0000558C File Offset: 0x0000378C
			public SimpleTextForm()
			{
				base.Width = 200;
				base.Height = 150;
				this.Text = "Goose \"Not-epad\"";
				TextBox textBox = new TextBox();
				textBox.Multiline = true;
				textBox.AcceptsReturn = true;
				textBox.Text = TheGoose.SimpleTextForm.possiblePhrases[TheGoose.SimpleTextForm.textIndices.Next()];
				textBox.Location = new Point(0, 0);
				textBox.Width = base.ClientSize.Width;
				textBox.Height = base.ClientSize.Height - 5;
				textBox.Select(textBox.Text.Length, 0);
				textBox.Font = new Font(textBox.Font.FontFamily, 10f, FontStyle.Regular);
				base.Controls.Add(textBox);
				string text = Environment.SystemDirectory + "\\notepad.exe";
				if (File.Exists(text))
				{
					try
					{
						base.Icon = Icon.ExtractAssociatedIcon(text);
						base.ShowIcon = true;
					}
					catch
					{
					}
				}
			}

			// Token: 0x06000090 RID: 144 RVA: 0x00002886 File Offset: 0x00000A86
			private void ExitWindow(object sender, EventArgs e)
			{
				base.Close();
			}

			// Token: 0x040000B8 RID: 184
			private static string[] possiblePhrases = new string[]
			{
				"am goose hjonk",
				"good work",
				"nsfdafdsaafsdjl\r\nasdas       sorry\r\nhard to type withh feet",
				"i cause problems on purpose",
				"\"peace was never an option\"\r\n   -the goose (me)",
				"\r\n\r\n  >o) \r\n    (_>"
			};

			// Token: 0x040000B9 RID: 185
			private static Deck textIndices = new Deck(TheGoose.SimpleTextForm.possiblePhrases.Length);
		}

		// Token: 0x0200001F RID: 31
		private class SimpleDonateForm : TheGoose.MovableForm
		{
			// Token: 0x06000092 RID: 146 RVA: 0x000056F8 File Offset: 0x000038F8
			public SimpleDonateForm()
			{
				new PictureBox();
				base.ClientSize = new Size((int)(250f * this.scale), (int)(300f * this.scale));
				try
				{
					this.BackgroundImage = Image.FromFile(TheGoose.SimpleDonateForm.donationGraphicSrc);
				}
				catch
				{
					Label label = new Label();
					label.Text = "Can't find the donation image... are you messing with the game files?\nCheck out my Twitter at twitter.com/samnchiet I guess?";
					label.Location = new Point(0, 0);
					label.Width = base.ClientSize.Width;
					label.Height = base.ClientSize.Height;
					label.BackColor = Color.White;
					label.TextAlign = ContentAlignment.MiddleCenter;
					base.Controls.Add(label);
				}
				this.BackgroundImageLayout = ImageLayout.Stretch;
				base.Controls.Add(this.SetupButton(111, 407, 390, 475, new EventHandler(this.OpenPatreonLink), true));
				base.Controls.Add(this.SetupButton(174, 500, 325, 545, new EventHandler(this.OpenPaypalLink), true));
				base.Controls.Add(this.SetupButton(381, 302, 433, 360, new EventHandler(this.OpenDiscordLink), true));
				base.Controls.Add(this.SetupButton(403, 247, 472, 312, new EventHandler(this.OpenTwitterLink), true));
			}

			// Token: 0x06000093 RID: 147 RVA: 0x00005898 File Offset: 0x00003A98
			private Button SetupButton(int point1X, int point1Y, int point2X, int point2Y, EventHandler handler, bool showHoverClick = true)
			{
				Button button = new Button();
				button.Location = new Point((int)((float)point1X * this.scale) / 2, (int)((float)point1Y * this.scale) / 2);
				button.Size = new Size((int)((float)(point2X - point1X) * this.scale) / 2, (int)((float)(point2Y - point1Y) * this.scale) / 2);
				button.Click += handler;
				button.Cursor = Cursors.Hand;
				button.BackColor = Color.Transparent;
				button.ForeColor = Color.Transparent;
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.MouseOverBackColor = (showHoverClick ? Color.FromArgb(40, Color.White) : Color.Transparent);
				button.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, Color.White);
				button.FlatAppearance.BorderSize = 0;
				button.TabStop = false;
				return button;
			}

			// Token: 0x06000094 RID: 148 RVA: 0x0000288E File Offset: 0x00000A8E
			private void OpenPatreonLink(object sender, EventArgs e)
			{
				Process.Start("https://www.patreon.com/bePatron?u=3541875");
			}

			// Token: 0x06000095 RID: 149 RVA: 0x0000289B File Offset: 0x00000A9B
			private void OpenPaypalLink(object sender, EventArgs e)
			{
				Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WUKYHY7SZ275Q&currency_code=USD&source=url");
			}

			// Token: 0x06000096 RID: 150 RVA: 0x000028A8 File Offset: 0x00000AA8
			private void OpenTwitterLink(object sender, EventArgs e)
			{
				Process.Start("https://www.twitter.com/samnchiet");
			}

			// Token: 0x06000097 RID: 151 RVA: 0x000028B5 File Offset: 0x00000AB5
			private void OpenDiscordLink(object sender, EventArgs e)
			{
				Process.Start("https://discord.gg/PCJS6DH");
			}

			// Token: 0x040000BA RID: 186
			private const string patreonLink = "https://www.patreon.com/bePatron?u=3541875";

			// Token: 0x040000BB RID: 187
			private const string paypalLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WUKYHY7SZ275Q&currency_code=USD&source=url";

			// Token: 0x040000BC RID: 188
			private const string twitterLink = "https://www.twitter.com/samnchiet";

			// Token: 0x040000BD RID: 189
			private const string discordLink = "https://discord.gg/PCJS6DH";

			// Token: 0x040000BE RID: 190
			private static string donationGraphicSrc = Program.GetPathToFileInAssembly("Assets/Images/OtherGfx/DonatePage.png");

			// Token: 0x040000BF RID: 191
			private float scale = 1.25f;
		}

		// Token: 0x02000020 RID: 32
		private struct Task_TrackMud
		{
			// Token: 0x06000099 RID: 153 RVA: 0x000028D3 File Offset: 0x00000AD3
			public static float GetDirChangeInterval()
			{
				return 100f;
			}

			// Token: 0x040000C0 RID: 192
			public const float DurationToRunAmok = 2f;

			// Token: 0x040000C1 RID: 193
			public float nextDirChangeTime;

			// Token: 0x040000C2 RID: 194
			public float timeToStopRunning;

			// Token: 0x040000C3 RID: 195
			public TheGoose.Task_TrackMud.Stage stage;

			// Token: 0x0200002A RID: 42
			public enum Stage
			{
				// Token: 0x040000F8 RID: 248
				DecideToRun,
				// Token: 0x040000F9 RID: 249
				RunningOffscreen,
				// Token: 0x040000FA RID: 250
				RunningWandering
			}
		}

		// Token: 0x02000021 RID: 33
		private struct Rig
		{
			// Token: 0x040000C4 RID: 196
			public const int UnderBodyRadius = 15;

			// Token: 0x040000C5 RID: 197
			public const int UnderBodyLength = 7;

			// Token: 0x040000C6 RID: 198
			public const int UnderBodyElevation = 9;

			// Token: 0x040000C7 RID: 199
			public Vector2 underbodyCenter;

			// Token: 0x040000C8 RID: 200
			public const int BodyRadius = 22;

			// Token: 0x040000C9 RID: 201
			public const int BodyLength = 11;

			// Token: 0x040000CA RID: 202
			public const int BodyElevation = 14;

			// Token: 0x040000CB RID: 203
			public Vector2 bodyCenter;

			// Token: 0x040000CC RID: 204
			public const int NeccRadius = 13;

			// Token: 0x040000CD RID: 205
			public const int NeccHeight1 = 20;

			// Token: 0x040000CE RID: 206
			public const int NeccExtendForward1 = 3;

			// Token: 0x040000CF RID: 207
			public const int NeccHeight2 = 10;

			// Token: 0x040000D0 RID: 208
			public const int NeccExtendForward2 = 16;

			// Token: 0x040000D1 RID: 209
			public float neckLerpPercent;

			// Token: 0x040000D2 RID: 210
			public Vector2 neckCenter;

			// Token: 0x040000D3 RID: 211
			public Vector2 neckBase;

			// Token: 0x040000D4 RID: 212
			public Vector2 neckHeadPoint;

			// Token: 0x040000D5 RID: 213
			public const int HeadRadius1 = 15;

			// Token: 0x040000D6 RID: 214
			public const int HeadLength1 = 3;

			// Token: 0x040000D7 RID: 215
			public const int HeadRadius2 = 10;

			// Token: 0x040000D8 RID: 216
			public const int HeadLength2 = 5;

			// Token: 0x040000D9 RID: 217
			public Vector2 head1EndPoint;

			// Token: 0x040000DA RID: 218
			public Vector2 head2EndPoint;

			// Token: 0x040000DB RID: 219
			public const int EyeRadius = 2;

			// Token: 0x040000DC RID: 220
			public const int EyeElevation = 3;

			// Token: 0x040000DD RID: 221
			public const float IPD = 5f;

			// Token: 0x040000DE RID: 222
			public const float EyesForward = 5f;
		}
	}
}
