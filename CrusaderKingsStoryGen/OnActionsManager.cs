using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrusaderKingsStoryGen
{
    class OnActionsManager
    {
        public static OnActionsManager instance = new OnActionsManager();

        public List<string> removals = new List<string>();

        public Script script;
        
        public void Load()
        {
            script = ScriptLoader.instance.Load(Globals.GameDir + "common\\on_actions\\00_on_actions.txt");

            removals.Add("on_character_switch_society_interest");
            removals.Add("on_character_ask_to_join_society");
            for (var i = 0; i < script.Root.Children.Count; i++)
            {
                var rootChild = script.Root.Children[i];
                if (rootChild is ScriptScope)
                {
                    var trigger = (rootChild as ScriptScope);
                    for (var index = 0; index < removals.Count; index++)
                    {
                        var removal = removals[index];
                        if (trigger.Name.Contains(removal))
                        {
                            script.Root.Remove(rootChild);
                            i--;
                            continue;
                        }
                    }
                }
            }

            string monasticBlock = "";


            script.Root.Do($@"
    
            on_character_switch_society_interest = {{
	            events = {{
		            MNM.9910010
		            MNM.9910024 
	            }}
            }}
            on_character_ask_to_join_society = {{
	            events = {{
		            MNM.994450 #Join Monastic Order
		            MNM.99100 #Join Hermetics
		            MNM.9910024 # Backup for refounding societies
	            }}
            }}


");

        }

        public void Save()
        {
            return;
            Script eventScript = EventManager.instance.GetNewScript("ck2gen_societyEvents");

            string interestedBlock = "";
            foreach (var instanceSecretSociety in SocietyManager.instance.secretSocieties)
            {
                interestedBlock += "interested_in_society = " + instanceSecretSociety + "\n";
            }

            eventScript.Root.Do($@"
            
            namespace = MNM

            character_event = {{
	            id = MNM.9910010
	            hide_window = yes

	            is_triggered_only = yes

	            trigger = {{
		            is_in_society = no
		            OR = {{
			            {interestedBlock}
		            }}
	            }}

	            immediate = {{
		            character_event = {{ id = MNM.9910011 days = 90 random = 640 }}
	            }}
            }}



    character_event = {{
	    id = MNM.9910024
	    desc = EVTDESC_MNM_10024
	    picture = GFX_evt_ritual_scroll
	    border = GFX_event_normal_frame_religion
	    is_triggered_only = yes
	    trigger = {{
		    NOT = {{
			    FROM = {{
				    any_society_member = {{
					    always = yes
				    }}
			    }}
		    }}
	    }}
	    immediate = {{
		    set_character_flag = society_join_block
	    }}
	    option = {{
		    name = EVTOPTA_MNM_10024
		    join_society = FROM
		    set_society_grandmaster = yes
		    add_society_currency_massive_effect = yes
		    clr_character_flag = society_join_block
	    }}
	    option = {{
		    name = EVTOPTB_MNM_10024
		    clr_character_flag = society_join_block
	    }}
    }}

"
                );

            String interestedTrigger = "";


            foreach (var instanceSecretSociety in SocietyManager.instance.secretSocieties)
            {
                interestedTrigger += $@"

	if = {{
			limit = {{
				interested_in_society = {instanceSecretSociety}
				NOT = {{ has_character_flag = ongoing_recruitment }}
			}}

			random_society_member = {{
				limit = {{
					society_member_of = {instanceSecretSociety}
					#is_within_diplo_range = ROOT
					is_society_grandmaster = no
				}}
				save_event_target_as = assassins_recruiter
				character_event = {{ id = MNM.9910012 }}
			}}
		}}
";
            }
            String monasticBlock = "";


            foreach (var instanceSecretSociety in SocietyManager.instance.monasticSocieties)
            {
                monasticBlock += $@"

					society_member_of = {instanceSecretSociety}	
";
            }

            eventScript.Root.Do($@"


character_event = {{
	id = MNM.9910011
	hide_window = yes

	is_triggered_only = yes

	trigger = {{
		is_in_society = no
		OR = {{
			{interestedBlock}
		}}
	}}

	immediate = {{
		
	        {interestedTrigger}
	
	}}	
}}


    character_event = {{
	id = MNM.994450
	hide_window = yes
	is_triggered_only = yes
	trigger = {{
		FROM = {{
			leader = {{
						OR = {{
		                    {monasticBlock}
	                    }}

			}}
		}}
	}}
	immediate = {{
		FROM = {{
			leader = {{
				character_event = {{
					id = MNM.994451
				}}
			}}
		}}
	}}
}}


character_event = {{
	id = MNM.994451
	hide_window = yes
	is_triggered_only = yes
	immediate = {{
		FROM = {{
			letter_event = {{
				id = MNM.994452
			}}
		}}
	}}
}}

letter_event = {{
	id = MNM.994452
	desc = EVTDESC_MNM_4452
	border = GFX_event_letter_frame_religion
	is_triggered_only = yes
	immediate = {{
		set_character_flag = society_join_block
	}}
	option = {{
		name = EVTOPTA_MNM_44511
		FROM = {{
			ROOT = {{
				join_prev_monastic_order_society = yes
			}}
		}}
		scaled_wealth = -0.25
		clr_character_flag = society_join_block
	}}
}}


    character_event = {{
	id = MNM.99100
	hide_window = yes
	is_triggered_only = yes
	trigger = {{
		FROM = {{
			leader = {{
				society_member_of = hermetics
			}}
		}}
	}}
	immediate = {{
		FROM = {{
			leader = {{
				character_event = {{
					id = MNM.101
				}}
			}}
		}}
	}}
}}

character_event = {{
	id = MNM.9910012
	hide_window = yes
	is_triggered_only = yes
	immediate = {{
		FROM = {{
			if = {{
				limit = {{
					NOT = {{
						has_character_flag = ongoing_recruitment
					}}
				}}
				set_character_flag = assassins_attempted_recruitment
				set_character_flag = ongoing_recruitment
				character_event = {{
						id = MNM.6022
					}}
			}}
		}}
	}}
}}

");
            eventScript.Save();
            script.Save();
        }
  
    }
}
