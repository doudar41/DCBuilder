// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
#if UNITY_POST_PROCESSING_STACK_V2
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess( typeof( BDColorPostProcessPPSRenderer ), PostProcessEvent.AfterStack, "BDColorPostProcess", true )]
public sealed class BDColorPostProcessPPSSettings : PostProcessEffectSettings
{
	[Tooltip( "PalletPosturize" )]
	public Vector4Parameter _PalletPosturize = new Vector4Parameter { value = new Vector4(1f,1f,1f,0f) };
	[Tooltip( "PixelScale" )]
	public FloatParameter _PixelScale = new FloatParameter { value = 1f };
}

public sealed class BDColorPostProcessPPSRenderer : PostProcessEffectRenderer<BDColorPostProcessPPSSettings>
{
	public override void Render( PostProcessRenderContext context )
	{
		var sheet = context.propertySheets.Get( Shader.Find( "BD/ColorPostProcess" ) );
		sheet.properties.SetVector( "_PalletPosturize", settings._PalletPosturize );
		sheet.properties.SetFloat( "_PixelScale", settings._PixelScale );
		context.command.BlitFullscreenTriangle( context.source, context.destination, sheet, 0 );
	}
}
#endif
