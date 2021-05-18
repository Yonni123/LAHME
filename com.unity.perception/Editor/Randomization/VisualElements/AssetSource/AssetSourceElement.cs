﻿using System;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEditor.Perception.Randomization;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Perception.Randomization;
using UnityEngine.UIElements;

namespace Editor.Randomization.VisualElements.AssetSource
{
    class AssetSourceElement : VisualElement
    {
        SerializedProperty m_AssetRoleProperty;
        SerializedProperty m_LocationProperty;
        ToolbarMenu m_AssetRoleToolbarMenu;
        ToolbarMenu m_LocationToolbarMenu;
        VisualElement m_FieldsContainer;
        TextElement m_LocationNotes;
        Type m_AssetType;

        AssetRoleBase assetRole =>
            (AssetRoleBase)StaticData.GetManagedReferenceValue(m_AssetRoleProperty);
        AssetSourceLocation assetSourceLocation =>
            (AssetSourceLocation)StaticData.GetManagedReferenceValue(m_LocationProperty);

        public AssetSourceElement(SerializedProperty property, FieldInfo fieldInfo)
        {
            m_AssetType = fieldInfo.FieldType.GetGenericArguments()[0];
            m_AssetRoleProperty = property.FindPropertyRelative("m_AssetRoleBase");
            m_LocationProperty = property.FindPropertyRelative(nameof(AssetSource<GameObject>.assetSourceLocation));
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                $"{StaticData.uxmlDir}/AssetSource/AssetSourceElement.uxml");
            template.CloneTree(this);

            var nameLabel = this.Q<Label>("name");
            nameLabel.text = property.displayName;

            m_FieldsContainer = this.Q<VisualElement>("fields-container");

            m_AssetRoleToolbarMenu = this.Q<ToolbarMenu>("asset-role-dropdown");
            var storedAssetRole = assetRole;
            m_AssetRoleToolbarMenu.text = storedAssetRole != null
                ? GetAssetRoleDisplayName(assetRole.GetType()) : "None";

            // ReSharper disable once PossibleNullReferenceException
            var baseType = fieldInfo.FieldType.GetProperty("assetRole").PropertyType;
            m_AssetRoleToolbarMenu.menu.AppendAction(
                "None",
                a => ReplaceAssetRole(null),
                a => DropdownMenuAction.Status.Normal);
            foreach (var type in StaticData.assetRoleTypes)
            {
                if (!type.IsSubclassOf(baseType))
                    continue;
                m_AssetRoleToolbarMenu.menu.AppendAction(
                    GetAssetRoleDisplayName(type),
                    a => ReplaceAssetRole(type),
                    a => DropdownMenuAction.Status.Normal);
            }

            m_LocationNotes = this.Q<TextElement>("location-notes");
            m_LocationToolbarMenu = this.Q<ToolbarMenu>("location-dropdown");
            foreach (var type in StaticData.assetSourceLocationTypes)
            {
                m_LocationToolbarMenu.menu.AppendAction(
                    GetDisplayName(type),
                    a => ReplaceLocation(type),
                    a => DropdownMenuAction.Status.Normal);
            }
            if (assetSourceLocation == null)
                CreateAssetSourceLocation(typeof(LocalAssetSourceLocation));
            UpdateLocationUI(assetSourceLocation.GetType());
        }

        void ReplaceAssetRole(Type type)
        {
            if (type == null)
            {
                m_AssetRoleToolbarMenu.text = "None";
                m_AssetRoleProperty.managedReferenceValue = null;
            }
            else
            {
                m_AssetRoleToolbarMenu.text = GetDisplayName(type);
                var newAssetRole = (AssetRoleBase)Activator.CreateInstance(type);
                m_AssetRoleProperty.managedReferenceValue = newAssetRole;
            }
            m_AssetRoleProperty.serializedObject.ApplyModifiedProperties();
        }

        void CreateAssetSourceLocation(Type type)
        {
            var newLocation = (AssetSourceLocation)Activator.CreateInstance(type);
            m_LocationProperty.managedReferenceValue = newLocation;
            m_LocationProperty.serializedObject.ApplyModifiedProperties();
        }

        void ReplaceLocation(Type type)
        {
            CreateAssetSourceLocation(type);
            UpdateLocationUI(type);
        }

        void UpdateLocationUI(Type type)
        {
            m_LocationToolbarMenu.text = GetDisplayName(type);
            var notesAttribute = (AssetSourceLocationNotes)Attribute.GetCustomAttribute(type, typeof(AssetSourceLocationNotes));
            if (notesAttribute != null)
            {
                m_LocationNotes.text = notesAttribute.notes;
                m_LocationNotes.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                m_LocationNotes.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            CreatePropertyFields();
        }

        void CreatePropertyFields()
        {
            m_FieldsContainer.Clear();
            if (m_LocationProperty.type == $"managedReference<{nameof(LocalAssetSourceLocation)}>")
                m_FieldsContainer.Add(
                    new AssetListElement(m_LocationProperty.FindPropertyRelative("assets"), m_AssetType));
            else
                UIElementsEditorUtilities.CreatePropertyFields(m_LocationProperty, m_FieldsContainer);
        }

        static string GetDisplayName(Type type)
        {
            var attribute = (DisplayNameAttribute)Attribute.GetCustomAttribute(type, typeof(DisplayNameAttribute));
            return attribute != null ? attribute.DisplayName : type.Name;
        }

        static string GetAssetRoleDisplayName(Type type)
        {
            return type.Name.Replace("AssetRole", string.Empty);
        }
    }
}
