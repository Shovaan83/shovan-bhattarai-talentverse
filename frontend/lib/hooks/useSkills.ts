import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { skillsApi } from "@/lib/api/skills";
import type { UserSkill, AddSkillPayload } from "@/lib/types/skills";

export const SKILLS_QUERY_KEY = ["my-skills"] as const;

export function useSkills() {
  return useQuery({
    queryKey: SKILLS_QUERY_KEY,
    queryFn: skillsApi.getMySkills,
  });
}

export function useAddSkill() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: skillsApi.addSkill,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SKILLS_QUERY_KEY });
    },
  });
}

export function useDeleteSkill() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: skillsApi.deleteSkill,
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: SKILLS_QUERY_KEY });

      const previousSkills = queryClient.getQueryData<UserSkill[]>(SKILLS_QUERY_KEY);

      queryClient.setQueryData<UserSkill[]>(SKILLS_QUERY_KEY, (old) =>
        old?.filter((skill) => skill.userSkillId !== deletedId)
      );

      return { previousSkills };
    },
    onError: (_err, _deletedId, context) => {
      queryClient.setQueryData(SKILLS_QUERY_KEY, context?.previousSkills);
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: SKILLS_QUERY_KEY });
    },
  });
}
