using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    partial class SocietyManager
    {
        public void CreateAssassinTemplate(ScriptScope parentSociety, ScriptScope parentScriptTrigger, String societyName, ReligionParser religion, ReligionGroupParser religiongroup=null)
        {
            if (religiongroup == null)
                religiongroup = religion.Group;

            parentSociety.Do($@"

{societyName} = {{
	primary_attribute = intrigue
	is_secret = yes
	devil_worshipper = no
	opinion_to_other_members = +10
	opinion_to_pretenders = -20			
	opinion_to_perceived_members = -20 
	opinion_per_rank_above = +10
	society_rank_up_decision = request_to_rank_up_within_society
	sound = assassins_interface
	society_ranks_gfx = generic_ranks
	
	active = {{
		has_dlc = ""Mystics""
	}}
	indestructible = yes

	can_join_society = {{
		hidden_trigger = {{
			NAND = {{
				ai = yes
				is_society_rank_full = {{ 
					society = {societyName}
					rank = 1
				}}
			}}
		}}
		has_{societyName}_prerequisites = yes
		hidden_trigger = {{
			NOT = {{ has_character_flag = society_join_block }}
		}}
	}}

	show_society = {{
		OR = {{
			religion = {religion.Name}
			secret_religion = {religion.Name}
		}}
		age = 16
		OR = {{
			is_female = no
			has_game_rule = {{
				name = gender
				value = all
			}}
		}}
	}}

	potential = {{
		OR = {{
			religion = {religion.Name}
			secret_religion = {religion.Name}
		}}
	}}

	society_rank = {{
		level = 1
		limit = 20 
		startup_limit = 10
		modifier = {{
			is_visible = {{
				OR = {{
					society_member_of = {societyName}
					FROM = {{ is_society_discovered = yes }}
				}}
			}}
			murder_plot_power_modifier = 0.1 # increased plotpower
		}}
		decisions = {{
			assassins_borrow_money
			assassins_donate
			assassins_drug_effect
		}}
	}}
	
	society_rank = {{
		level = 2
		limit = 15
		startup_limit = 6
		modifier = {{ 
			is_visible = {{
				OR = {{
					society_member_of = {societyName}
					FROM = {{ is_society_discovered = yes }}
				}}
			}}
			murder_plot_power_modifier = 0.2 # further increased plotpower
			defensive_plot_power_modifier = 0.1
			intrigue = 1 
			combat_rating = 1 # bonus to dueling
		}}
		decisions = {{
			assassins_scare_ruler #Available (targeted) Power: Can scare rulers to get a Favor (leave a dagger on their pillow, etc. 
		}}
	}}
	
	society_rank = {{
		level = 3
		limit = 5 
		startup_limit = 3
		modifier = {{
			is_visible = {{
				OR = {{
					society_member_of = {societyName}
					FROM = {{ is_society_discovered = yes }}
				}}
			}}
			murder_plot_power_modifier = 0.30 # further increased plotpower
			defensive_plot_power_modifier = 0.15
			intrigue = 2 # further increase
			combat_rating = 1 # bonus to dueling (left over)
			plot_discovery_chance = 0.1 # bonus to plot discovery chance
		}}
		decisions = {{
			assassins_raise_ships #Available Power: Can raise special fleet of Ships
		}}
	}}

	society_rank = {{
		level = 4
		limit = 1 
		startup_limit = 1
		modifier = {{
			is_visible = {{
				OR = {{
					society_member_of = {societyName}
					FROM = {{ is_society_discovered = yes }}
				}}
			}}
			murder_plot_power_modifier = 0.50 # further increased plotpower
			defensive_plot_power_modifier = 0.25
			intrigue = 3 # further increase
			combat_rating = 1 # bonus to dueling (left over)
			plot_discovery_chance = 0.2 # further bonus to plot discovery chance
		}}
		decisions = {{
			assassins_raise_troops
			assassins_mark_for_death 
		}}
		obedience_tooltip = obedience_tooltip 
		obedient = {{
			ROOT = {{
				always = yes
			}}
		}}
	}}

	monthly_currency_gain = {{
		name = currency_name_{societyName}
    
		per_attribute = {{
			name = intrigue 
			value = 0.5
		}}
		has_trait = {{
			trait = zealous
			value = 3
		}}
		has_trait = {{
			trait = erudite
			value = 3
		}}
		has_trait = {{
			trait = scholar
			value = 3
		}}
		has_trait = {{
			trait = theologian
			value = 3
		}}
		has_trait = {{
			trait = schemer
			value = 3
		}}
		has_trait = {{
			trait = strong
			value = 1
		}}
		has_trait = {{
			trait = robust
			value = 1
		}}
		has_trait = {{
			trait = genius
			value = 1
		}}
		has_trait = {{
			trait = quick
			value = 1
		}}
		has_trait = {{
			trait = shrewd
			value = 1
		}}
		has_trait = {{
			trait = diligent
			value = 1
		}}
		has_trait = {{
			trait = patient
			value = 1
		}}
		has_trait = {{
			trait = temperate
			value = 1
		}}
		has_trait = {{
			trait = charitable
			value = 1
		}}
		society_rank = {{
			rank = 4
			value = 10
		}}
	}}

	member_score_per_attribute = 3
	member_score_per_rank = 100

	member_score = {{
		value = 10


		modifier = {{
			trait = zealous
			factor = 1.3
		}}
		modifier = {{
			trait = erudite
			factor = 1.3
		}}
		modifier = {{
			trait = scholar
			factor = 1.3
		}}
		modifier = {{
			trait = theologian
			factor = 1.3
		}}
		modifier = {{
			trait = schemer
			factor = 1.3
		}}
		modifier = {{
			trait = strong
			factor = 1.1
		}}
		modifier = {{
			trait = robust
			factor = 1.1
		}}
		modifier = {{
			trait = genius
			factor = 1.1
		}}
		modifier = {{
			trait = quick
			factor = 1.1
		}}
		modifier = {{
			trait = shrewd
			factor = 1.1
		}}
		modifier = {{
			trait = diligent
			factor = 1.1
		}}
		modifier = {{
			trait = patient
			factor = 1.1
		}}
		modifier = {{
			trait = temperate
			factor = 1.1
		}}
		modifier = {{
			trait = charitable
			factor = 1.1
		}}	
	}}
	
	startup_populate = {{
	    trigger = {{
			ai = yes
	    	controls_religion = no 
			religion = {religion.Name}
			age = 16
			OR = {{
				is_female = no
				has_game_rule = {{
					name = gender
					value = all
				}}
			}}
			NOT = {{ trait = decadent }}
			OR = {{
				trait = zealous
				trait = schemer
				trait = elusive_shadow
				trait = deceitful
				trait = ambitious
				intrigue = 18
			}}
			is_in_society = no
			NOT = {{ higher_tier_than = DUKE }}
			NOT = {{ mercenary = yes }}
		}}
	}}
}}
");

            parentScriptTrigger.Do($@"
            has_{societyName}_prerequisites = {{
                age = 16
	            true_religion_{religion.Name}_trigger = yes
	                OR = {{
		                is_female = no
		                has_game_rule = {{
			                name = gender
			                value = all
		                }}
	                }}
                }}
            ");


            secretSocieties.Add((parentSociety.Children[parentSociety.Children.Count-1] as ScriptScope).Name);
        }
    }
}
