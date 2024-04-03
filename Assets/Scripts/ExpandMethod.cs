using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

namespace D.Unity3dTools
{
    /// <summary>
    /// 组件扩展类
    /// </summary>
    public static class ExpandMethod
    {
        #region EventTrigger
        /// <summary>
        /// 通过代码添加EventTrigger事件（只能做unity已经定义的事件）
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="eventID"></param>
        /// <param name="call"></param>
        public static void AddTrggerEventListener(this EventTrigger trigger, EventTriggerType eventID, Action<PointerEventData> call)
        {
            TriggerEvent callback = new TriggerEvent();
            callback.AddListener(data => call(data as PointerEventData));
            Entry entry = new Entry() { callback = callback, eventID = eventID };
            trigger.triggers.Add(entry);
        }
        /// <summary>
        /// 通过代码删除EventTrigger事件（只能做unity已经定义的事件）
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="eventID"></param>
        public static void RemoveEventListener(this EventTrigger trigger, EventTriggerType eventID)
        {
            List<Entry> newTriggers = new List<Entry>();
            foreach (Entry entry in trigger.triggers)
            {
                if (entry.eventID == eventID) continue;
                newTriggers.Add(entry);
            }
            trigger.triggers = newTriggers;
        }
        /// <summary>
        /// 通过代码删除全部EventTrigger事件
        /// </summary>
        /// <param name="trigger"></param>
        public static void RemoveAllEventListener(this EventTrigger trigger)
        {
            trigger.triggers.RemoveAll((item) => { return item != null; });
        }
        #endregion

        #region Transform
        /// <summary>
        /// 重置Transform的本地尺寸，角度以及位置
        /// </summary>
        /// <param name="value"></param>
        public static void Reset(this Transform value)
        {
            value.position = Vector3.zero;
            value.localPosition = Vector3.zero;
            value.localRotation = Quaternion.identity;
            value.localScale = Vector3.one;
        }
        /// <summary>
        /// 删除所有子物体
        /// </summary>
        /// <param name="value"></param>
        public static void RemoveAllChildren(this Transform value)
        {
            for (int i = value.childCount - 1; i >= 0; i--)
            {
                Transform child = value.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }
        /// <summary>
        /// 获取组件，如果没有组件，则添加组件并返回新组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this Transform value) where T : Component
        {
            T component;
            bool isExist = value.gameObject.TryGetComponent<T>(out component);
            if (!isExist) component = value.gameObject.AddComponent<T>();
            return component;
        }
        /// <summary>
        /// 根据当前鼠标位置显示RectTransform
        /// </summary>
        /// <param name="self">需要设定位置的对象</param>
        /// <param name="offset">在鼠标点击位置的基础上添加的位移</param>
        public static void SetRectPosByMousePos(this RectTransform self, Vector2 offset)
        {
            Vector2 pivot = self.pivot;
            float rectW = self.rect.width;
            float rectH = self.rect.height;

            Rect screenRect = Screen.safeArea;
            float rootW = screenRect.size.x;
            float rootH = screenRect.size.y;

            Vector2 mousePos = Input.mousePosition;
            Vector2 tempPos = mousePos - 0.5f * screenRect.size;

            Vector2 luPivot = Vector2.up;
            Vector2 rdPivot = Vector2.right;
            Vector2 luPoint = tempPos + offset + new Vector2((luPivot.x - pivot.x) * rectW, (luPivot.y - pivot.y) * rectH);
            Vector2 rdPoint = tempPos + offset + new Vector2((rdPivot.x - pivot.x) * rectW, (rdPivot.y - pivot.y) * rectH);

            Vector2 reviseOffset = Vector2.zero;
            if (luPoint.x < -0.5f * rootW) reviseOffset += new Vector2(-0.5f * rootW - luPoint.x, 0);
            if (luPoint.y > 0.5f * rootH) reviseOffset += new Vector2(0, 0.5f * rootH - luPoint.y);
            if (rdPoint.x > 0.5f * rootW) reviseOffset += new Vector2(0.5f * rootW - rdPoint.x, 0);
            if (rdPoint.y < -0.5f * rootH) reviseOffset += new Vector2(0, -0.5f * rootH - rdPoint.y);

            tempPos += reviseOffset + offset;
            self.localPosition = tempPos;
        }
        #endregion

        #region Texture
        /// <summary>
        /// 将Texture2D转换成Sprite
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Sprite ToSprite(this Texture2D self)
        {
            Rect rect = new Rect(0, 0, self.width, self.height);
            Vector2 pivot = Vector2.one * 0.5f;
            return Sprite.Create(self, rect, pivot);
        }
        /// <summary>
        /// 捕获指定区域的屏幕截图
        /// </summary>
        /// <param name="captureArea">要捕获的区域</param>
        /// <param name="captureCamera">用于截图的相机</param>
        /// <param name="bytes">保存截图的字节数组</param>
        public static void CaptureScreenshot(Rect captureArea, Camera captureCamera,out byte[] bytes) 
        {
            // 保存原相机设置
            Rect originalRect = captureCamera.rect;
            RenderTexture originalRT = captureCamera.targetTexture;

            // 设置相机和RenderTexture用于截图
            captureCamera.rect = captureArea;
            RenderTexture rt = new RenderTexture((int)captureArea.width, (int)captureArea.height, 24);
            captureCamera.targetTexture = rt;

            // 创建一个2D纹理，读取截图像素
            Texture2D screenShot = new Texture2D((int)captureArea.width, (int)captureArea.height, TextureFormat.RGB24, false);
            captureCamera.Render();
            RenderTexture.active = rt;
            Vector2 startPos = 0.5f * new Vector2(Screen.width - captureArea.width, Screen.height - captureArea.height);
            screenShot.ReadPixels(new Rect(startPos.x, startPos.y, captureArea.x, captureArea.y), 0, 0);
            captureCamera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(rt);

            // 恢复相机原设置
            captureCamera.rect = originalRect;
            captureCamera.targetTexture = originalRT;

            // 将Texture2D转换为字节数组
           bytes = screenShot.EncodeToPNG();
        }
        #endregion

        #region Collection
        /// <summary>
        /// 去除列表中的重复对象，支持单一判断条件
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="ts"></param>
        /// <param name="checkFunc"></param>
        public static void RemoveDuplicates<T1, T2>(this IList<T1> ts, Func<T1, T2> checkFunc)
        {
            IList<T2> temp = new List<T2>();
            List<int> indexList = new List<int>();

            for (int i = 0; i < ts.Count; i++)
            {
                T1 t1 = ts[i];
                T2 t2 = checkFunc(t1);
                if (temp.Contains(t2)) indexList.Add(i);
                else temp.Add(t2);
            }

            for (int i = indexList.Count; i > 0; --i)
            {
                ts.RemoveAt(indexList[i]);
            }
        }
        /// <summary>
        /// 查找列表中符合条件的item数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">目标集合</param>
        /// <param name="selectFunc">查找函数</param>
        /// <returns></returns>
        public static int GetSelectNum<T>(this ICollection<T> ts, Func<T, bool> selectFunc)
        {
            int count = 0;
            foreach (T t in ts)
            {
                if (selectFunc(t)) count++;
            }
            return count;
        }
        /// <summary>
        /// 返回集合中符合条件的集合
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="ts">原集合</param>
        /// <param name="selectFunc">查找函数</param>
        /// <returns>所有符合条件的集合</returns>
        public static ICollection<T> GetSelectCollection<T>(this ICollection<T> ts, Func<T, bool> selectFunc)
        {
            ICollection<T> result = new HashSet<T>();
            foreach (T t in ts)
            {
                if (selectFunc(t)) result.Add(t);
            }
            return result;
        }
        /// <summary>
        /// 获得字典的某一项
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static KeyValuePair<TKey, TValue> GetPair<TKey, TValue>(this Dictionary<TKey, TValue> dic, int index)
        {
            if (dic != null && dic.Count > index && index >= 0)
            {
                // 将字典的键值对转换为列表，然后获取指定索引的元素
                List<KeyValuePair<TKey, TValue>> keyValueList = new List<KeyValuePair<TKey, TValue>>(dic);

                return keyValueList[index];
            }

            // 如果字典为空或索引无效，返回默认值
            return default(KeyValuePair<TKey, TValue>);
        }
        /// <summary>
        /// 将键和值添加或替换到字典中
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key) == false) dic.Add(key, value);
            else dic[key] = value;
        }
        #endregion

        #region Reflection
        /// <summary>
        /// 对引用类型的深度拷贝
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2">被复制对象类型</typeparam>
        /// <param name="destination"></param>
        /// <param name="source">被复制对象</param>
        /// <param name="isPublicOnly">是否只复制被复制对象的公有变量</param>
        public static void CopyFrom<T1, T2>(this T1 destination, T2 source, bool isPublicOnly = true)
        {
            Type type1 = typeof(T1);
            Type type2 = typeof(T2);

            BindingFlags flags_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            BindingFlags flags_Public = BindingFlags.Public | BindingFlags.Instance;

            PropertyInfo[] properties1 = type1.GetProperties(isPublicOnly ? flags_Public : flags_all);
            PropertyInfo[] properties2 = type2.GetProperties(isPublicOnly ? flags_Public : flags_all);

            FieldInfo[] fieldInfos1 = type1.GetFields(isPublicOnly ? flags_Public : flags_all);
            FieldInfo[] fieldInfos2 = type2.GetFields(isPublicOnly ? flags_Public : flags_all);


            foreach (FieldInfo field1 in fieldInfos1)
            {
                foreach (FieldInfo field2 in fieldInfos2)
                {
                    if (field1.Name == field2.Name && field1.FieldType == field2.FieldType)
                    {
                        if (field2.FieldType.IsValueType || field2.FieldType.Equals(typeof(string)))
                        {
                            field1.SetValue(destination, field2.GetValue(source));
                            break;
                        }
                        else if (IsListType(field2.FieldType))
                        {
                            field1.SetValue(destination, field2.GetValue(source));
                        }
                        else
                        {
                            object retval = Activator.CreateInstance(field1.FieldType);
                            retval.CopyFrom(field2.GetValue(source));
                            field1.SetValue(destination, retval);
                            break;
                        }
                    }
                }
            }

            foreach (PropertyInfo prop1 in properties1)
            {
                foreach (PropertyInfo prop2 in properties2)
                {
                    if (prop1.Name == prop2.Name && prop1.PropertyType == prop2.PropertyType)
                    {
                        if (prop1.PropertyType.IsValueType)
                        {
                            prop1.SetValue(destination, prop2.GetValue(source));
                            break;
                        }
                        else
                        {
                            object retval = Activator.CreateInstance(prop1.PropertyType);
                            retval.CopyFrom(prop2.GetValue(source));
                            prop1.SetValue(destination, retval);
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 判断对象是否是一个列表
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static bool IsListType(Type fieldType)
        {
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region TimeCountdown
        public struct CountdownData
        {
            public int Days;
            public int Hours;
            public int Minutes;
        }
        public static CountdownData CalculateCountdown(long timestampMillis)
        {
            DateTime currentTime = DateTime.Now;
            DateTime targetTime = DateTimeOffset.FromUnixTimeMilliseconds(timestampMillis).LocalDateTime;

            TimeSpan timeLeft = targetTime - currentTime;

            CountdownData countdownData = new CountdownData
            {
                Days = timeLeft.Days,
                Hours = timeLeft.Hours % 24,
                Minutes = timeLeft.Minutes % 60,
            };

            return countdownData;
        }
        #endregion

#if Has_DG_Tweening
        #region DoTween
        public static void DoFadeIn(this Transform trans, float duration = 0.2f, float targetAlpha = 1)
        {
            trans.gameObject.SetActive(true);
            CanvasGroup canvasGroup = trans.GetOrAddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            // 设置动画对象id
            string tweenId = canvasGroup.GetInstanceID() + "_DoFade";
            DOTween.Kill(tweenId);
            // 使用Tween.To来实现渐变
            canvasGroup.alpha = 0;
            Tweener tweener = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, targetAlpha, duration).SetId(tweenId)
                .SetEase(Ease.Linear)
                 .OnComplete(() =>
                 {
                     canvasGroup.blocksRaycasts = true;
                 });
        }
        public static void DoFadeOut(this Transform trans, float duration = 0.2f, float targetAlpha = 0)
        {
            trans.gameObject.SetActive(true);
            CanvasGroup canvasGroup = trans.gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            // 设置动画对象id
            string tweenId = canvasGroup.GetInstanceID() + "_DoFade";
            DOTween.Kill(tweenId);
            // 使用Tween.To来实现渐变
            Tweener tweener = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, targetAlpha, duration).SetId(tweenId)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    trans.gameObject.SetActive(false);
                });
        }
        #endregion
#endif
    }
}