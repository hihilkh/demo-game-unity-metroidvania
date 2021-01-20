Shader "Char/StencilBuffer" {
    SubShader {
        Tags { "Queue"="Transparent-1" "RenderType"="Transparent" }

		Pass {
        	Stencil {
            	Comp Always
            	Pass IncrSat
        	}

			ZWrite Off
        	ColorMask 0
    	}
    }
}