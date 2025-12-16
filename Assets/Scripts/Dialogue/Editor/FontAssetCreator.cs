using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;
using System.Text;

namespace CivilizationJourney.Dialogue.Editor
{
    /// <summary>
    /// 字体资源快速创建工具
    /// 右键点击TTF/OTF字体文件，选择"创建TMP字体资源"即可生成
    /// </summary>
    public class FontAssetCreator
    {
        /// <summary>
        /// 获取常用中文字符集
        /// </summary>
        private static string GetChineseCharacters()
        {
            StringBuilder sb = new StringBuilder();
            
            // ASCII可打印字符 (32-126)
            for (int i = 32; i <= 126; i++)
            {
                sb.Append((char)i);
            }
            
            // 中文标点符号
            sb.Append("\u3002\uFF0C\u3001\uFF1B\uFF1A\uFF1F\uFF01"); // 。，、；：？！
            sb.Append("\u201C\u201D\u2018\u2019"); // ""''
            sb.Append("\uFF08\uFF09\u3010\u3011\u300A\u300B"); // （）【】《》
            sb.Append("\u2014\u2026\u00B7"); // —…·
            
            // 常用汉字（约3500字）
            string commonChinese = 
                "的一是不了在人有我他这个们中来上大为和国地到以说时要就出会可也你对生能而子那得于着下自之年过发后作里用道行所然家种事" +
                "成方多经么去法学如都同现当没动面起看定天分还进好小部其些主样理心她本前开但因只从想实日军者意无力它与长把机十民第公此已工使" +
                "情明性知全三又关点正业外将两高间由问很最重并物手应战向头文体政美相见被利什二等产或新己制身果加西斯月话合回特代内信表化老给" +
                "世位次度门任常先海通教儿原东声提立及比员解水名真论处走义各入几口认条平系气题活尔更别打女变四神总何电数安少报才结反受目太量" +
                "再感建务做接必场件计管期市直德资命山金指克许统区保至队形社便空决治展马科司五基眼书非则听白却界达光放强即像难且权思王象完设" +
                "式色路记南品住告类求据程北边死张该交规万取拉格望觉术领共确传师观清今切院让识候带导争运笑飞风步改收根干造言联持组每济车亲极" +
                "林服快办议往元英士证近失转夫令准布始怎呢存未远叫台单影具罗字爱击流备兵连调深商算质团集百需价花党华城石级整府离况亚请技际约" +
                "示复病息究线似官火断精满支视消越器容照须九增研写称企八功吗包片史委乎查轻易早曾除农找装广显吧阿李标谈吃图念六引历首医局突专" +
                "费号尽另周较注语仅考落青随选列武红响虽推势参希古众构房半节土投某案黑维革划敌致陈律足态护七兴派孩验责营星够章音跟志底站严巴" +
                "例防族供效续施留讲型料终答紧黄绝奇察母京段依批群项故按河米围江织害斗双境客纪采举杀攻父苏密低朝友诉止细愿千值仍男钱破网热助" +
                "倒育属坐帝限船脸职速刻乐否刚威毛状率甚独球般普怕弹校苦创假久错承阻运药诗灵够险曲压延迷春右叶卫厂睛盘角降维杨敢绿模块酒守待" +
                "尸染弱困季脚尼迎居危致陷卷烟悲脱硬愈遭介抱鬼哪套概念索顿疑练押缩继环宗急血著雷健盟杂孙宝岛托修静松困异汉坏适墨欲缺标谓供销" +
                "存票般杯简渐篇预减阶审沉坚善妈刘退房启密批戏宜枪登暗洲抗朱灭幸础弃亿游微庄季训控激娘纳喝遍盖副坦牌江副忘铁朋池临欧纸缓凡执" +
                "纯毒缘遇怀抓扩副凭圣恶劳暴博洋欢狂端忙泪童脏宣宁鲜帮梦袭洗疗遗憾";
            
            sb.Append(commonChinese);
            
            return sb.ToString();
        }

        [MenuItem("Assets/创建TMP字体资源/常用中文字符集", false, 1000)]
        private static void CreateTMPFontAssetChinese()
        {
            CreateTMPFontAsset(GetChineseCharacters(), 2048, 48);
        }

        [MenuItem("Assets/创建TMP字体资源/ASCII字符集", false, 1001)]
        private static void CreateTMPFontAssetASCII()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 32; i <= 126; i++)
            {
                sb.Append((char)i);
            }
            CreateTMPFontAsset(sb.ToString(), 512, 32);
        }

        [MenuItem("Assets/创建TMP字体资源/扩展中文字符集", false, 1002)]
        private static void CreateTMPFontAssetFullChinese()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetChineseCharacters());
            
            // 添加CJK基本区汉字 (U+4E00 - U+9FFF)
            for (int i = 0x4E00; i <= 0x9FFF; i++)
            {
                sb.Append((char)i);
            }
            
            CreateTMPFontAsset(sb.ToString(), 4096, 36);
        }

        [MenuItem("Assets/创建TMP字体资源/常用中文字符集", true)]
        [MenuItem("Assets/创建TMP字体资源/ASCII字符集", true)]
        [MenuItem("Assets/创建TMP字体资源/扩展中文字符集", true)]
        private static bool ValidateFontAsset()
        {
            var selected = Selection.activeObject;
            if (selected == null) return false;

            string path = AssetDatabase.GetAssetPath(selected);
            string ext = Path.GetExtension(path).ToLower();
            return ext == ".ttf" || ext == ".otf" || ext == ".ttc";
        }

        private static void CreateTMPFontAsset(string characters, int atlasSize, int samplingSize)
        {
            string fontPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

            if (sourceFont == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择一个有效的字体文件", "确定");
                return;
            }

            string directory = Path.GetDirectoryName(fontPath);
            string fontName = Path.GetFileNameWithoutExtension(fontPath);
            string outputPath = Path.Combine(directory, fontName + " SDF.asset");

            if (File.Exists(outputPath))
            {
                if (!EditorUtility.DisplayDialog("文件已存在", 
                    fontName + " SDF.asset 已存在，是否覆盖？", "覆盖", "取消"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(outputPath);
            }

            EditorUtility.DisplayProgressBar("创建TMP字体资源", "正在生成字体图集...", 0.3f);

            try
            {
                // 创建字体资源
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    samplingSize,
                    9,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                    atlasSize,
                    atlasSize,
                    AtlasPopulationMode.Dynamic
                );

                if (fontAsset == null)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("错误", "创建字体资源失败", "确定");
                    return;
                }

                EditorUtility.DisplayProgressBar("创建TMP字体资源", "正在添加字符...", 0.6f);

                // 添加字符
                string missingChars;
                fontAsset.TryAddCharacters(characters, out missingChars);

                if (!string.IsNullOrEmpty(missingChars))
                {
                    int showCount = Mathf.Min(50, missingChars.Length);
                    Debug.LogWarning("部分字符在字体中不存在: " + missingChars.Substring(0, showCount) + "...");
                }

                EditorUtility.DisplayProgressBar("创建TMP字体资源", "正在保存...", 0.9f);

                // 保存资源
                AssetDatabase.CreateAsset(fontAsset, outputPath);

                if (fontAsset.atlasTexture != null)
                {
                    fontAsset.atlasTexture.name = fontName + " Atlas";
                    AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                }

                if (fontAsset.material != null)
                {
                    fontAsset.material.name = fontName + " Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();

                var createdAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath);
                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);

                EditorUtility.DisplayDialog("成功", 
                    "TMP字体资源已创建:\n" + outputPath, "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", "创建失败: " + e.Message, "确定");
                Debug.LogError(e);
            }
        }
    }
}
