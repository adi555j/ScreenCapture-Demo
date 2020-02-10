using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Display;
using Android.Media.Projection;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using Log = CommonSampleLibrary.Log;
using Android.Media;

namespace ScreenCapture
{
	// Provides UI for the screen capture
	public class ScreenCaptureFragment : Fragment
	{
		private const string TAG = "ScreenCaptureFragment";
		private const string STATE_RESULT_CODE = "result_code";
		private const string STATE_RESULT_DATA = "result_data";

		private const int REQUEST_MEDIA_PROJECTION = 1;

		private int screenDensity;

		private int resultCode;
		private Intent resultData;

		private Surface surface;
		private MediaProjection mediaProjection;
		private VirtualDisplay virtualDisplay;
		private MediaProjectionManager mediaProjectionManager;
		private Button buttonToggle;
		private SurfaceView surfaceView;
		public MediaRecorder mMediaRecorder { get; set; }


		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			mMediaRecorder = new MediaRecorder();
			if (savedInstanceState != null)
			{
				resultCode = savedInstanceState.GetInt(STATE_RESULT_CODE);
				resultData = (Intent)savedInstanceState.GetParcelable(STATE_RESULT_DATA);
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.fragment_screen_capture, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			
			surfaceView = view.FindViewById<SurfaceView>(Resource.Id.surface);

			surface = surfaceView.Holder.Surface;

			buttonToggle = view.FindViewById<Button>(Resource.Id.toggle);
			buttonToggle.Click += delegate (object sender, EventArgs e) {
				if (virtualDisplay == null)
					StartScreenCapture();
				else
					StopScreenCapture();
			};
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			var metrics = new DisplayMetrics();
			Activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
			screenDensity = (int)metrics.DensityDpi;
			mediaProjectionManager = (MediaProjectionManager)Activity.GetSystemService(Context.MediaProjectionService);
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			if (resultData != null)
			{
				outState.PutInt(STATE_RESULT_CODE, resultCode);
				outState.PutParcelable(STATE_RESULT_DATA, resultData);
			}
		}

		public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == REQUEST_MEDIA_PROJECTION)
			{
				if (resultCode != Result.Ok)
				{
					Log.Info(TAG, "User cancelled");
					Toast.MakeText(Activity, Resource.String.user_cancelled, ToastLength.Short).Show();
					return;
				}
				if (Activity == null)
					return;

				Log.Info(TAG, "Starting screem capture");
				this.resultCode = (int)resultCode;
				this.resultData = data;
				SetUpMediaProjection();
				SetUpVirtualDisplay();
				
			}
		}

		public override void OnPause()
		{
			base.OnPause();
			StopScreenCapture();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (mMediaRecorder != null)
			{
				mMediaRecorder.Release();
				mMediaRecorder.Dispose();
				mMediaRecorder = null;
			}
			TearDownMediaProjection();
		}

		private void SetUpMediaProjection()
		{
			mediaProjection = mediaProjectionManager.GetMediaProjection(resultCode, resultData);

		}

		private void TearDownMediaProjection()
		{
			if (mediaProjection != null)
			{
				mediaProjection.Stop();
				mediaProjection = null;
			}
		}

		private void StartScreenCapture()
		{
			try
			{
				if (surface == null || Activity == null)
					return;
				if (mediaProjection != null)
				{
					SetUpVirtualDisplay();
				}
				else if (resultCode != 0 && resultData != null)
				{
					SetUpMediaProjection();
					SetUpVirtualDisplay();

				}
				else
				{
					Log.Info(TAG, "Requesting confirmation");
					// This initiates a prompt for the user to confirm screen projection.
					StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), REQUEST_MEDIA_PROJECTION);
				}
			}
			catch (Exception e)
			{

			}

		}

		private void SetUpVirtualDisplay()
		{
			Log.Info(TAG, "Setting up a VirtualDisplay: " + surfaceView.Width + "x" + surfaceView.Height + " (" + screenDensity + ")");
			
			virtualDisplay = mediaProjection.CreateVirtualDisplay("ScreenCapture",
				surfaceView.Width, surfaceView.Height, screenDensity,
				(DisplayFlags)VirtualDisplayFlags.AutoMirror, surface, null, null);

			buttonToggle.SetText(Resource.String.stop);
			StartRecording();
		}

		private void StopScreenCapture()
		{
			try
			{
				if (virtualDisplay == null)
					return;

				
				
				buttonToggle.SetText(Resource.String.start);
				//mMediaRecorder.Stop();
				//mMediaRecorder.Release();
				virtualDisplay.Release();
				virtualDisplay = null;

			}
			catch (Exception e)
			{

			}

		}
		//[get: Android.Runtime.Register("getSurface", "()Landroid/view/Surface;", "GetGetSurfaceHandler", ApiSince = 21)]
		//public virtual Android.Views.Surface Surface { get; }
		private void StartRecording()
		{
			try
			{
					string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/test.mp4";
					mMediaRecorder.SetVideoSource(VideoSource.Surface);
					mMediaRecorder.SetOutputFormat(OutputFormat.Webm);
					//mMediaRecorder.SetVideoEncodingBitRate(512 * 1000);
					mMediaRecorder.SetVideoEncoder(VideoEncoder.Vp8);
					mMediaRecorder.SetVideoSize(640, 480);
					//mMediaRecorder.SetVideoFrameRate(10);
					mMediaRecorder.SetOutputFile(path);
					mMediaRecorder.Prepare();
					mMediaRecorder.Start();


			}
			catch (Exception e)
			{

			}


		}



	}
}
