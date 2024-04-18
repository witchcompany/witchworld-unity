using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Module;
using WitchCompany.Toolkit.Validation;
using Object = UnityEngine.Object;

namespace WitchCompany.Toolkit.Editor.Validation
{
    public static class OptimizationValidator
    {
        /// <summary>
        /// 최적화 관련 유효성 검사
        /// - 배치된 오브젝트 전체 가져오기
        /// - 배치된 개별 매쉬 버텍스 개수
        /// - 배치된 전체 메쉬 버텍스 개수
        /// - 사용된 라이트맵 용량
        /// - 사용된 오디오 클립 용량
        /// - 가장 용량이 큰 오디오 클립
        /// - 사용된 텍스쳐 용량
        /// - 가장 용량이 큰 텍스쳐
        /// - 유니크 머테리얼 개수
        /// - light 타입, 모드 검사
        /// - reflection Probe 용량
        /// </summary>
        public static ValidationReport ValidationCheck()
        {
            // 유효성 검사 결과
            var validationReport = new ValidationReport();

            /* Scene Vital */
            // 전체 버텍스 검사
            // var meshVertex = GetAllMeshes().Item2;
            // if (meshVertex > OptimizationConfig.MaxVerts)
            // {
            //     var error = new ValidationError($"Total Mesh Vertex Count : {meshVertex} / { OptimizationConfig.MaxVerts}\n" +
            //                                     $"모든 Vertex의 최대 개수는 {OptimizationConfig.MaxVerts}개 입니다. Scene 내의 Mesh Vertex를 조절해주세요.", ValidationTag.TagMeshVertex);
            //     validationReport.Append(error);
            // }
            
            //  라이트맵 검사
            var lightMapSize = GetLightMapMB();
            if (lightMapSize > OptimizationConfig.MaxLightmapMb)
            {
                var error = new ValidationError(
                    $"Total Light Map Size : {lightMapSize} / {OptimizationConfig.MaxLightmapMb} MB\n" +
                    $"모든 Light Map의 최대 크기는 {OptimizationConfig.MaxLightmapMb} MB입니다. Scene에 적용된 Light Map을 조절해주세요.",
                    ValidationTag.TagLightmap);
                validationReport.Append(error);
            }

            // 오디오 클립 검사
            var findAudioClips = FindAudioClipList();
            var audioClipSize = GetAudioClipMB(findAudioClips);
            if (audioClipSize > OptimizationConfig.MaxAudioClipMb)
            {
                var error = new ValidationError(
                    $"Total Audio Clip Size : {audioClipSize} / {OptimizationConfig.MaxAudioClipMb} MB\n" +
                    $"모든 Audio Clip의 최대 크기는 {OptimizationConfig.MaxAudioClipMb} MB입니다. Scene에 적용된 Audio Clip을 조절해주세요.",
                    ValidationTag.TagAudioClip);
                validationReport.Append(error);
            }

            GetLargestAudioClipArray(findAudioClips);

            //  텍스쳐 검사
            var findTexture = FindTextureList();
            var textureSize = GetTextureMB(findTexture);
            if (textureSize > OptimizationConfig.MaxSharedTextureMb)
            {
                var error = new ValidationError(
                    $"Texture Size : {textureSize} / {OptimizationConfig.MaxSharedTextureMb} MB\n" +
                    $"모든 Texture의 최대 크기는 {OptimizationConfig.MaxSharedTextureMb} MB입니다. Scene 내의 Texture를 조절해주세요.",
                    ValidationTag.TagTexture);
                validationReport.Append(error);
            }

            GetLargestTextureArray(findTexture);

            //  유니크 머티리얼 검사
            var materialCount = GetUniqueMaterialCount();
            if (materialCount > OptimizationConfig.MaxUniqueMaterials)
            {
                var error = new ValidationError(
                    $"Unique Material Count : {materialCount} / {OptimizationConfig.MaxUniqueMaterials}\n" +
                    $"Material의 최대 개수는 {OptimizationConfig.MaxUniqueMaterials}개입니다. Scene 내에 적용된 Material의 수를 조절해주세요.",
                    ValidationTag.TagMaterial);
                validationReport.Append(error);
            }

            /* ValidationCheck 버튼 눌렸을 때만 진행 */
            // validationReport.Append(ValidateIndividualMeshVertex());
            validationReport.Append(ValidateMeshCollider());
            validationReport.Append(ValidateLight());
            validationReport.Append(ValidateReflectionProbe());
            validationReport.Append(ValidationAudioClipPreset(findAudioClips));
            validationReport.Append(ValidationUseCrunchCompression(findTexture));
            // validationReport.Append(ScriptRuleValidator.ValidateMissingComponents(SceneManager.GetActiveScene()));

            return validationReport;
        }

        /// <summary> 개별 mesh의 vertex 개수 검사 </summary>
        private static ValidationReport ValidateIndividualMeshVertex()
        {
            var report = new ValidationReport();

            var meshes = GetAllMeshes().Item1;
            // var uniqueMeshes = meshes.Distinct();

            foreach (var mesh in meshes)
            {
                if (mesh.Item2.vertexCount > OptimizationConfig.MaxIndividualVerts)
                {
                    var error = new ValidationError(
                        $"{mesh.Item1.name}의 {mesh.Item2.name} : {mesh.Item2.vertexCount} / {OptimizationConfig.MaxIndividualVerts}\n" +
                        $"오브젝트의 최대 Vertex 개수는 {OptimizationConfig.MaxIndividualVerts}입니다. 해당 오브젝트의 Mesh를 수정해주세요.",
                        ValidationTag.TagMeshVertex, mesh.Item1);
                    report.Append(error);
                }
            }

            return report;
        }

        /// <summary> 전체 mesh의 vertex 개수 검사 </summary>
        public static (List<Tuple<GameObject, Mesh>>, int) GetAllMeshes()
        {
            var meshes = new List<Tuple<GameObject, Mesh>>();
            int totalVertexCount = 0;

            // 모든 게임 오브젝트 중 모든 랜더러를 배열에 저장
            var renderers = GameObject.FindObjectsOfType<Renderer>(true);

            // 랜더러 배열 탐색
            foreach (var renderer in renderers)
            {
                if (renderer is MeshRenderer)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null)
                    {
                        meshes.Add(new Tuple<GameObject, Mesh>(filter.gameObject, filter.sharedMesh));
                        totalVertexCount += filter.sharedMesh.vertexCount;
                    }
                }
                else if (renderer is SkinnedMeshRenderer)
                {
                    var skinned = renderer as SkinnedMeshRenderer;
                    if (skinned.sharedMesh != null)
                    {
                        meshes.Add(new Tuple<GameObject, Mesh>(skinned.gameObject, skinned.sharedMesh));
                        totalVertexCount += skinned.sharedMesh.vertexCount;
                    }
                }
                else if (renderer is BillboardRenderer)
                {
                    meshes.Add(new Tuple<GameObject, Mesh>(renderer.gameObject, null));
                    totalVertexCount += 4;
                }
            }

            return (meshes, totalVertexCount);
        }

        /// <summary> 라이트맵 용량 : 소수점 아래 3번째 자리까지 표시 </summary>
        public static double GetLightMapMB()
        {
            double bytes = 0;
            var lightmaps = LightmapSettings.lightmaps;

            foreach (var lightmap in lightmaps)
            {
                if (lightmap.lightmapColor != null)
                {
                    long sizeInBytes = Profiler.GetRuntimeMemorySizeLong(lightmap.lightmapColor);
                    bytes += sizeInBytes;
                }

                if (lightmap.lightmapDir != null)
                {
                    long sizeInBytes = Profiler.GetRuntimeMemorySizeLong(lightmap.lightmapDir);
                    bytes += sizeInBytes;
                }

                if (lightmap.shadowMask != null)
                {
                    long sizeInBytes = Profiler.GetRuntimeMemorySizeLong(lightmap.shadowMask);
                    bytes += sizeInBytes;
                }
            }

            return Math.Round(bytes / 1024 / 1024, 3);
        }

        /// <summary> 오디오 클립 불러오기 </summary>
        public static IEnumerable<AudioClip> FindAudioClipList()
        {
            var blockManager = Object.FindObjectOfType<WitchBlockManager>(true);
            if (blockManager == null)
                return null;

            var loadSoundEffects = new List<AudioClip>();
            var soundEffects = Object.FindObjectsOfType<WitchSoundEffect>(true);

            // Witch Sound Effect의 Target Clip (Audio Clip) 탐색 -> Audio Clip 찾아서 리스트에 추가 (foundAudioClip)
            foreach (var soundEffect in soundEffects)
            {
                var audioClip = soundEffect.TargetClip;

                if (audioClip != null)
                    loadSoundEffects.Add(audioClip);
            }

            var bgmClip = blockManager.DefaultBGM;

            if (bgmClip != null)
                loadSoundEffects.Add(bgmClip);

            return loadSoundEffects.Distinct();
        }

        /// <summary> 전체 오디오 클립 용량 : 소숫점 아래 3번째 자리까지 표시 </summary>
        public static double GetAudioClipMB(IEnumerable<AudioClip> audioClipList)
        {
            if (audioClipList == null) return 0;

            var foundAudioClips = audioClipList;

            // 오디오 클립 사이즈 계산
            var totalBytes = 0L;


            foreach (var audioClip in foundAudioClips)
            {
                var sizeInBytes = Profiler.GetRuntimeMemorySizeLong(audioClip);
                // Debug.Log($"[Audio Clip]{audioClip.name}: {Math.Round(sizeInBytes/1024f/1024f)}MB");
                totalBytes += sizeInBytes;
            }

            const double mb = 1024D * 1024D;
            return Math.Round(totalBytes / mb, 3);
        }

        /// <summary> 가장 용량이 큰 오디오 클립: 5개까지 표시 </summary>
        public static void GetLargestAudioClipArray(IEnumerable<AudioClip> audioClipList)
        {
            if (audioClipList == null) return;

            var foundAudioClips = audioClipList;
            var audioClipNames = new[] { "", "", "", "", "" };
            var audioClipSizes = new[] { 0D, 0, 0, 0, 0 };

            // 가장 용량이 큰 오디오 클립 로깅
            foreach (var audioClip in foundAudioClips)
            {
                var sizeInBytes = Profiler.GetRuntimeMemorySizeLong(audioClip);

                if (!(audioClipSizes[^1] < sizeInBytes)) continue;
                audioClipNames[^1] = audioClip.name;
                audioClipSizes[^1] = sizeInBytes;

                for (var i = audioClipSizes.Length - 1; i > 0; i--)
                {
                    if (!(audioClipSizes[i] > audioClipSizes[i - 1])) continue;
                    (audioClipNames[i], audioClipNames[i - 1]) = (audioClipNames[i - 1], audioClipNames[i]);
                    (audioClipSizes[i], audioClipSizes[i - 1]) = (audioClipSizes[i - 1], audioClipSizes[i]);
                }
            }

            for (var i = 0; i < audioClipSizes.Length; i++)
            {
                if (audioClipSizes[i] > 0f)
                    Debug.Log($"[Audio Clip]{audioClipNames[i]}: {Math.Round(audioClipSizes[i] / 1024 / 1024, 3)}MB");
            }
        }

        /// <summary> 텍스쳐 불러오기 </summary>>
        public static IEnumerable<Texture> FindTextureList()
        {
            var loadTextures = new List<Texture>();
            var renderers = Object.FindObjectsOfType<Renderer>(true);

            foreach (var renderer in renderers)
            {
                // 랜더러의 shardMaterial 배열 탐색
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null)
                        continue;

                    // 머티리얼의 texture 배열 탐색 -> texture 찾아서 리스트에 추가 (foundTextures)
                    foreach (var texName in material.GetTexturePropertyNames())
                    {
                        var tex = material.GetTexture(texName);
                        if (tex != null)
                        {
                            loadTextures.Add(tex);
                        }
                    }
                }
            }

            return loadTextures.Distinct();
        }

        /// <summary> 전체 텍스쳐 용량 : 소숫점 아래 3번째 자리까지 표시 </summary>
        public static double GetTextureMB(IEnumerable<Texture> textureList)
        {
            var foundTextures = textureList;

            // 텍스쳐 사이즈 계산
            var totalBytes = 0L;
            foreach (var texture in foundTextures)
            {
                var sizeInBytes = Profiler.GetRuntimeMemorySizeLong(texture);
                // Debug.Log($"[Texture]{texture.name}: {Math.Round(sizeInBytes/1024f/1024f)}MB", texture);
                totalBytes += sizeInBytes;
            }

            const double mb = 1024D * 1024D;
            return Math.Round(totalBytes / mb, 3);
        }

        /// <summary> 가장 용량이 큰 텍스쳐 : 5개까지 표시 </summary>
        public static void GetLargestTextureArray(IEnumerable<Texture> textureList)
        {
            var foundTextures = textureList;

            var textureNames = new[] { "", "", "", "", "" };
            var textureSizes = new[] { 0D, 0, 0, 0, 0 };

            // 가장 용량이 큰 텍스쳐 로깅
            foreach (var texture in foundTextures.Distinct())
            {
                var sizeInBytes = Profiler.GetRuntimeMemorySizeLong(texture);

                if (!(textureSizes[^1] < sizeInBytes)) continue;
                textureNames[^1] = texture.name;
                textureSizes[^1] = sizeInBytes;

                for (var i = textureSizes.Length - 1; i > 0; i--)
                {
                    if (!(textureSizes[i] > textureSizes[i - 1])) continue;
                    (textureNames[i], textureNames[i - 1]) = (textureNames[i - 1], textureNames[i]);
                    (textureSizes[i], textureSizes[i - 1]) = (textureSizes[i - 1], textureSizes[i]);
                }
            }

            for (var i = 0; i < textureSizes.Length; i++)
            {
                if (textureSizes[i] > 0f)
                    Debug.Log($"[Texture]{textureNames[i]}: {Math.Round(textureSizes[i] / 1024 / 1024, 3)}MB");
            }
        }

        /// <summary> 유니크 머테리얼 개수 </summary>
        public static int GetUniqueMaterialCount()
        {
            List<Material> materials = new List<Material>();

            Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                // 머티리얼 리스트에 랜더러의 shardMaterial 추가
                materials.AddRange(renderer.sharedMaterials);
            }

            return materials.FindAll(m => m != null).Select(m => m.name).Distinct().Count();
            ;
        }

        /// <summary> Mesh Collider를 가진 Object 검출 </summary>
        private static ValidationReport ValidateMeshCollider()
        {
            var report = new ValidationReport();

            var meshColliders = Object.FindObjectsOfType<MeshCollider>(true);
            var meshCount = 0;
            foreach (var meshCollider in meshColliders)
            {
                // 페인트에서 사용중이지 않은 것만 검사
                if (meshCollider.GetComponent<WitchPaintWall>() == null)
                    meshCount++;
            }

            if (meshCount > OptimizationConfig.MaxMeshColliderCount)
            {
                var errorMessage = $"Mesh Collider의 최대 개수는 {OptimizationConfig.MaxMeshColliderObject}개입니다." +
                                   $" ({meshColliders.Length} / {OptimizationConfig.MaxMeshColliderObject})\n" +
                                   $"Scene 내에 적용된 Mesh Collider의 수를 조절해주세요.";
                report.Append(new ValidationError(errorMessage, ValidationTag.TagMeshCollider));

                foreach (var meshCol in meshColliders)
                {
                    if (meshCol.sharedMesh != null)
                    {
                        var error = new ValidationError($"Object : {meshCol.gameObject.name}",
                            ValidationTag.TagMeshCollider, meshCol);
                        report.Append(error);
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// Light 유효성 검사
        /// - Directional light 1개, Point, Spot light 총합 2개로 light 개수 제한
        /// - directional, point light를 제외한 realtime light 검출
        /// - light mode가 baked인 light 중 활성화 된 오브젝트 검출
        /// </summary>
        private static ValidationReport ValidateLight()
        {
            var report = new ValidationReport();
            var lights = Object.FindObjectsOfType<Light>(true);
            var directionalLights = new List<Light>();
            var realtimeLights = new List<Light>();
            var etcLights = new List<Light>();
            var disabledBakedLights = new List<Light>();

            const string realTimeLightErrorMsg =
                "Directional, Point, Spot Light를 제외한 Light의 Mode를 RealTime으로 지정할 수 없습니다.";
            const string bakedLightErrorMsg = "Light의 Mode가 Baked인 오브젝트는 비활성화 되어야 합니다.";

            // light 검출
            foreach (var light in lights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Baked)
                {
                    if (light.gameObject.activeSelf)
                        disabledBakedLights.Add(light);
                    continue;
                }

                switch (light.type)
                {
                    case LightType.Directional:
                        directionalLights.Add(light);
                        break;
                    case LightType.Point:
                        realtimeLights.Add(light);
                        break;
                    case LightType.Spot:
                        realtimeLights.Add(light);
                        break;
                    default:
                        etcLights.Add(light);
                        break;
                }
            }

            LightTypeErrorMessage(report, directionalLights, ValidationTag.TagDirectionalLight,
                OptimizationConfig.MaxDirectionalLight);
            LightTypeErrorMessage(report, realtimeLights, ValidationTag.TagRealtimeLight,
                OptimizationConfig.MaxRealtimeLight);

            LightModeErrorMessage(report, etcLights, ValidationTag.TagEtcRealtimeLight, realTimeLightErrorMsg);
            LightModeErrorMessage(report, disabledBakedLights, ValidationTag.TagBaked, bakedLightErrorMsg);

            return report;
        }

        private static void LightTypeErrorMessage(ValidationReport report, List<Light> lights, string tag,
            uint maxCount)
        {
            if (lights.Count <= maxCount) return;

            var newTag = "Light Type : " + tag;

            var errorMessage =
                $"{tag} Light의 최대 개수는 {maxCount}개입니다. " +
                $"({lights.Count} / {maxCount})\n" +
                $"Scene 내에 적용된 {tag} Light의 수를 조절해주세요.";
            report.Append(new ValidationError(errorMessage, newTag));

            foreach (var light in lights)
                report.Append(new ValidationError($"Object : {light.name}", newTag, light));
        }

        private static void LightModeErrorMessage(ValidationReport report, List<Light> lights, string tag, string msg)
        {
            if (lights.Count <= 0) return;
            var newTag = "Light Mode : " + tag;

            report.Append(new ValidationError(msg, newTag));

            foreach (var light in lights)
                report.Append(new ValidationError($"Object : {light.name}", newTag, light));
        }


        /// <summary> Reflection Probe 용량 검사 </summary>
        private static ValidationReport ValidateReflectionProbe()
        {
            var report = new ValidationReport();

            long bytes = 0;
            var reflectionProbes = GameObject.FindObjectsOfType<ReflectionProbe>(true);
            foreach (var probe in reflectionProbes)
            {
                if (probe.mode == UnityEngine.Rendering.ReflectionProbeMode.Baked && probe.texture != null)
                {
                    bytes += Profiler.GetRuntimeMemorySizeLong(probe.texture);
                }
                else if (probe.mode == UnityEngine.Rendering.ReflectionProbeMode.Realtime)
                {
                    bytes += probe.resolution * probe.resolution * 3;
                }
            }

            int mBytes = (int)bytes / 1024 / 1024;
            if (mBytes > OptimizationConfig.MaxReflectionProbeMb)
            {
                var error = new ValidationError(
                    $"Size : {mBytes}/{OptimizationConfig.MaxReflectionProbeMb} MB\n모든 Reflection Probe의 최대 크기는 {OptimizationConfig.MaxReflectionProbeMb}입니다. ",
                    ValidationTag.TagReflectionProbe);
                report.Append(error);
            }

            return report;
        }

        /// <summary> Audio Clip Preset 사용 여부 검사 </summary>
        private static ValidationReport ValidationAudioClipPreset(IEnumerable<AudioClip> audioClipList)
        {
            var report = new ValidationReport();
            if (audioClipList == null) return report;

            var foundAudioClips = audioClipList;

            // Audio Clip Preset 적용 확인
            foreach (var audioClip in foundAudioClips)
            {
                var path = AssetDatabase.GetAssetPath(audioClip);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;

                if (importer == null) continue;

                // Preset 적용 확인할 if문
                if (importer.forceToMono == true && importer.loadInBackground == false && importer.ambisonic == false &&
                    audioClip.loadType == AudioClipLoadType.CompressedInMemory &&
                    audioClip.preloadAudioData == true) continue;
                if (importer.forceToMono == false && importer.loadInBackground == true && importer.ambisonic == false &&
                    audioClip.loadType == AudioClipLoadType.Streaming) continue;

                var error = new ValidationError($"Audio Clip: {audioClip.name}\nAudio Clip Preset을 적용해야 합니다.",
                    ValidationTag.TagAudioClip, audioClip);
                report.Append(error);
            }

            return report;
        }

        /// <summary> Use Crunch Compression 사용 여부 검사 </summary>
        private static ValidationReport ValidationUseCrunchCompression(IEnumerable<Texture> textureList)
        {
            var report = new ValidationReport();
            if (textureList == null) return report;

            var foundTextures = textureList;

            // crunch compress 설정 확인
            foreach (var texture in foundTextures)
            {
                var path = AssetDatabase.GetAssetPath(texture);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null) continue;
                if (importer.crunchedCompression) continue;

                var error = new ValidationError($"Texture: {texture.name}\nCrunch Compression 기능을 활성화해야 합니다.",
                    ValidationTag.TagTexture, texture);
                report.Append(error);
            }

            return report;
        }
    }
}