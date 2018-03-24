﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.PARAMS;
using OpenTK;
using Smash_Forge.Rendering.Lights;

namespace Smash_Forge.Params
{
    public class LightSetParam
    {
        private ParamFile paramFile;

        public DirectionalLight characterDiffuse;
        public DirectionalLight characterDiffuse2;
        public DirectionalLight characterDiffuse3;

        // The first 4 lights are character lights.
        public DirectionalLight[] stageDiffuseLights = new DirectionalLight[68];

        public Vector3[] stageFogSet = new Vector3[16];

        public HemisphereFresnel fresnelLight;

        public LightSetParam(string fileName)
        {
            paramFile = new ParamFile(fileName);
            for (int i = 0; i < stageDiffuseLights.Length; i++)
            {
                stageDiffuseLights[i] = CreateDirectionalLightFromLightSet(paramFile, i, "Stage " + (i + 1));
            }

            for (int i = 0; i < stageFogSet.Length; i++)
            {
                stageFogSet[i] = CreateFogColorFromFogSet(paramFile, i);
            }

            characterDiffuse = CreateCharDiffuseLightFromLightSet(paramFile);
            characterDiffuse2 = CreateDirectionalLightFromLightSet(paramFile, 0, "Diffuse2");
            characterDiffuse3 = CreateDirectionalLightFromLightSet(paramFile, 1, "Diffuse3");
            fresnelLight = CreateFresnelLightFromLightSet(paramFile);
        }

        public void Save(string fileName)
        {
            // TODO: Update all the light values.
            SaveFresnelLight();
            SaveCharDiffuseLights();
            for (int i = 0; i < 16; i++)
            {
                SaveFogColor(i);
            }
            // The first 4 lights are character lights.
            for (int i = 4; i < stageDiffuseLights.Length; i++)
            {
                SaveDirectionalLight(i);
            }

            paramFile.Export(fileName);
        }

        public static HemisphereFresnel CreateFresnelLightFromLightSet(ParamFile lightSet)
        {
            Vector3 hsvGround = new Vector3(0);
            hsvGround.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 8);
            hsvGround.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 9);
            hsvGround.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 10);

            Vector3 hsvSky = new Vector3(0);
            hsvSky.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 11);
            hsvSky.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 12);
            hsvSky.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 13);

            float skyAngle = (float)ParamTools.GetParamValue(lightSet, 0, 0, 14);
            float groundAngle = (float)ParamTools.GetParamValue(lightSet, 0, 0, 15);

            return new HemisphereFresnel(hsvGround, hsvSky, skyAngle, groundAngle, "Fresnel");
        }

        private void SaveFresnelLight()
        {
            ParamTools.ModifyParamValue(paramFile, 0, 0, 8, fresnelLight.groundColor.H);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 9, fresnelLight.groundColor.S);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 10, fresnelLight.groundColor.V);

            ParamTools.ModifyParamValue(paramFile, 0, 0, 11, fresnelLight.skyColor.H);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 12, fresnelLight.skyColor.S);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 13, fresnelLight.skyColor.V);

        }

        public static DirectionalLight CreateCharDiffuseLightFromLightSet(ParamFile lightSet)
        {
            Vector3 diffuseHsv = new Vector3(0);
            diffuseHsv.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 29);
            diffuseHsv.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 30);
            diffuseHsv.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 31);

            Vector3 ambientHsv = new Vector3(0);
            ambientHsv.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 33);
            ambientHsv.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 34);
            ambientHsv.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 35);

            float rotX = (float)ParamTools.GetParamValue(lightSet, 1, 4, 5);
            float rotY = (float)ParamTools.GetParamValue(lightSet, 1, 4, 6);
            float rotZ = (float)ParamTools.GetParamValue(lightSet, 1, 4, 7);

            return new DirectionalLight(diffuseHsv, ambientHsv, 0, 0, 0, "Diffuse");
        }

        private void SaveCharDiffuseLights()
        {
            ParamTools.ModifyParamValue(paramFile, 0, 0, 29, characterDiffuse.diffuseColor.H);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 30, characterDiffuse.diffuseColor.S);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 31, characterDiffuse.diffuseColor.V);

            ParamTools.ModifyParamValue(paramFile, 0, 0, 33, characterDiffuse.ambientColor.H);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 34, characterDiffuse.ambientColor.S);
            ParamTools.ModifyParamValue(paramFile, 0, 0, 35, characterDiffuse.ambientColor.V);
            
            // TODO: Save light rotations.

            for (int i = 0; i < 4; i++)
            {
                SaveDirectionalLight(i);
            }
        }

        public static Vector3 CreateFogColorFromFogSet(ParamFile lightSet, int fogIndex)
        {
            // First fog is probably for characters.
            float hue = (float)ParamTools.GetParamValue(lightSet, 2, 1 + fogIndex, 0);
            float saturation = (float)ParamTools.GetParamValue(lightSet, 2, 1 + fogIndex, 1);
            float value = (float)ParamTools.GetParamValue(lightSet, 2, 1 + fogIndex, 2);
            float fogR = 0.0f, fogB = 0.0f, fogG = 0.0f;
            ColorTools.HsvToRgb(hue, saturation, value, out fogR, out fogG, out fogB);
            Vector3 color = new Vector3(fogR, fogG, fogB);
            return color;
        }

        private void SaveFogColor(int fogIndex)
        {
            // First fog is probably for characters.
            ParamTools.ModifyParamValue(paramFile, 2, 1 + fogIndex, 0, stageFogSet[fogIndex].X);
            ParamTools.ModifyParamValue(paramFile, 2, 1 + fogIndex, 1, stageFogSet[fogIndex].Y);
            ParamTools.ModifyParamValue(paramFile, 2, 1 + fogIndex, 2, stageFogSet[fogIndex].Z);
        }

        public static DirectionalLight CreateDirectionalLightFromLightSet(ParamFile lightSet, int lightIndex, string name)
        {
            bool enabled = (uint)ParamTools.GetParamValue(lightSet, 1, lightIndex, 1) == 1;
            Vector3 hsv = new Vector3(0);
            hsv.X = (float)ParamTools.GetParamValue(lightSet, 1, lightIndex, 2);
            hsv.Y = (float)ParamTools.GetParamValue(lightSet, 1, lightIndex, 3);
            hsv.Z = (float)ParamTools.GetParamValue(lightSet, 1, lightIndex, 4);

            float rotX = (float)ParamTools.GetParamValue(lightSet, 1, lightIndex, 5);
            float rotY = (float)ParamTools.GetParamValue(lightSet, 1, lightIndex, 6);
            float rotZ = (float)ParamTools.GetParamValue(lightSet, 1, lightIndex, 7);

            DirectionalLight newLight = new DirectionalLight(hsv, new Vector3(0), rotX, rotY, rotZ, name);
            newLight.enabled = enabled; // doesn't render properly for some stages
            return newLight;
        }

        private void SaveDirectionalLight(int lightIndex)
        {
            // TODO: Enabled/disabled for light.
            ParamTools.ModifyParamValue(paramFile, 1, lightIndex, 2, stageDiffuseLights[lightIndex].diffuseColor.H);
            ParamTools.ModifyParamValue(paramFile, 1, lightIndex, 3, stageDiffuseLights[lightIndex].diffuseColor.S);
            ParamTools.ModifyParamValue(paramFile, 1, lightIndex, 4, stageDiffuseLights[lightIndex].diffuseColor.V);

            ParamTools.ModifyParamValue(paramFile, 1, lightIndex, 5, stageDiffuseLights[lightIndex].rotX);
            ParamTools.ModifyParamValue(paramFile, 1, lightIndex, 6, stageDiffuseLights[lightIndex].rotY);
            ParamTools.ModifyParamValue(paramFile, 1, lightIndex, 7, stageDiffuseLights[lightIndex].rotZ);
        }
    }
}
