import { z } from "zod/v4";

export const InsuranceClaimSchema = z.object({
  claim_id: z.string().meta({description: "Policy or claim number"}),
  claimant: z.object({
    name: z.string().meta({description: "Full name of the person filing the claim"}),
    email: z.email().meta({description: "Email address of the claimant"}),
  }).meta({description: "Information about the person filing the claim"}),
  incident: z.object({
    date: z.iso.date().meta({description: "Date when the incident occurred"}),
    location: z.string().meta({description: "Location where the incident happened"}),
    type: z.enum(["collision", "theft", "vandalism", "weather", "other"]).meta({description: "Type of incident"}),
    description: z.string().meta({description: "Brief description of what happened"}),
    fault_determination: z.enum(["claimant", "other_party", "no_fault", "disputed", "unknown"]).meta({description: "Who was at fault"}),
    injuries: z.boolean().meta({description: "Whether there were any injuries"}),
    police_report: z.boolean().meta({description: "Whether a police report was filed"}),
  }).meta({description: "Details about the incident"}),
  claim_status: z.enum(["filed", "under_review", "approved", "denied", "closed"]).meta({description: "Current status of the claim"}),
  assigned_adjuster: z.object({
    name: z.string().meta({description: "Name of the assigned claims adjuster"}),
    email: z.email().meta({description: "Email of the assigned adjuster"}),
  }).meta({description: "Information about the assigned adjuster"}),
  inspection_scheduled: z.boolean().meta({description: "Whether a vehicle inspection has been scheduled"}),
  inspection_date: z.iso.date().meta({description: "Date of scheduled inspection"}),
  timeline: z.array(z.object({
    date: z.iso.date().meta({description: "Date of the event"}),
    event: z.string().meta({description: "Description of what happened"}),
    participant: z.string().meta({description: "Who was involved in this event"}),
  })).meta({description: "Timeline of events in the claim process"}),
  resolution: z.object({
    approved: z.boolean().meta({description: "Whether the claim was approved"}),
  }).meta({description: "Final resolution of the claim"}),
});
