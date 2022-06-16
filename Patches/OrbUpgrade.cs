using HarmonyLib;
using TMPro;
using UI.PostBattle;
using UnityEngine;
using UnityEngine.UI;

namespace OrbRandomizer.Patches
{
    [HarmonyPatch(typeof(UpgradeDetailsManager), nameof(UpgradeDetailsManager.PopulateUpgradeDetails))]
    public static class OrbUpgrade
    {
        public static bool Prefix(UpgradeDetailsManager __instance, Attack before, Attack after)
        {
			before.ClearBattleParameters();
			before.SoftInit(__instance._deckManager, __instance._relicManager, __instance._cruciballManager);
			foreach (Transform transform in __instance._elementsToClear)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			__instance._elementsToClear.Clear();
			__instance._orbTitle.text = before.Name;
			if (after == null)
			{
				__instance.CreateSimpleIconDetails(false, before.GetModifiedDamagePerPeg(0).ToString(), before.GetDamageModMultiplier(0));
				__instance.CreateSimpleIconDetails(true, before.GetModifiedDamagePerPeg(1).ToString(), before.GetDamageModMultiplier(1));
				__instance.CreateDescription(before.Description, "");
				return false;
			}
			after.ClearBattleParameters();
			after.SoftInit(__instance._deckManager, __instance._relicManager, __instance._cruciballManager);
			if (before.GetModifiedDamagePerPeg(0) != after.GetModifiedDamagePerPeg(0))
			{
				__instance.CreateUpgradeIconDetails(false, before.GetModifiedDamagePerPeg(0).ToString(), after.GetModifiedDamagePerPeg(0).ToString(), before.GetDamageModMultiplier(0), after.GetDamageModMultiplier(0));
			}
			else
			{
				__instance.CreateSimpleIconDetails(false, before.GetModifiedDamagePerPeg(0).ToString(), before.GetDamageModMultiplier(0));
			}
			if (before.GetModifiedDamagePerPeg(1) != after.GetModifiedDamagePerPeg(1))
			{
				__instance.CreateUpgradeIconDetails(true, before.GetModifiedDamagePerPeg(1).ToString(), after.GetModifiedDamagePerPeg(1).ToString(), before.GetDamageModMultiplier(0), after.GetDamageModMultiplier(0));
			}
			else
			{
				__instance.CreateSimpleIconDetails(true, before.GetModifiedDamagePerPeg(1).ToString(), before.GetDamageModMultiplier(1));
			}
			__instance.CreateDescription(before.Description, after.Description);
			return false;
        }
    }

	[HarmonyPatch(typeof(UpgradeConfirmationPanel), nameof(UpgradeConfirmationPanel.PopulateDataForOrbUpgrade))]
	public static class ShowUpgradedOrbImage
    {
		public static void Postfix(UpgradeConfirmationPanel __instance, GameObject orb)
        {
			GameObject previous = __instance.transform.GetChild(0).gameObject;

			Attack attack = orb.GetComponent<Attack>();
			if(attack != null && attack.NextLevelPrefab != null)
            {
				// Orb Image
				GameObject next;
				if(__instance.transform.childCount == 3)
                {
					next = __instance.transform.GetChild(2).gameObject;
				} else
                {
					next = GameObject.Instantiate(previous, previous.transform.parent);

					Vector3 pos = previous.transform.position;
					previous.transform.position = new Vector3(pos.x - 3, pos.y, pos.z);
					next.transform.position = new Vector3(pos.x + 3, pos.y, pos.z);
				}
				next.GetComponentInChildren<TextMeshProUGUI>().text = attack.NextLevelPrefab.GetComponent<Attack>().LevelAsRomanNumeral;
				PachinkoBall nextPachinko = attack.NextLevelPrefab.GetComponent<PachinkoBall>();
				Image image = next.transform.GetChild(0).GetComponent<Image>();
				image.sprite = nextPachinko.sprite;
				image.color = nextPachinko.color;

				// Orb Title
				GameObject background = __instance.transform.GetChild(1).GetChild(0).gameObject;
				GameObject title = background.transform.GetChild(0).gameObject;

				title.SetActive(false);
				
			}

		}
    }
}
