import { z } from "zod";

export const InsuranceClaimSchema = z.object({
  claim_id: z.string().describe("Policy or claim number"),
  claimant: z.object({
    name: z.string().describe("Full name of the person filing the claim"),
    email: z.string().email().describe("Email address of the claimant"),
  }).describe("Information about the person filing the claim"),
  incident: z.object({
    date: z.string().date().describe("Date when the incident occurred"),
    location: z.string().describe("Location where the incident happened"),
    type: z.enum(["collision", "theft", "vandalism", "weather", "other"]).describe("Type of incident"),
    description: z.string().describe("Brief description of what happened"),
    fault_determination: z.enum(["claimant", "other_party", "no_fault", "disputed", "unknown"]).describe("Who was at fault"),
    injuries: z.boolean().describe("Whether there were any injuries"),
    police_report: z.boolean().describe("Whether a police report was filed"),
  }).describe("Details about the incident"),
  claim_status: z.enum(["filed", "under_review", "approved", "denied", "closed"]).describe("Current status of the claim"),
  assigned_adjuster: z.object({
    name: z.string().describe("Name of the assigned claims adjuster"),
    email: z.string().email().describe("Email of the assigned adjuster"),
  }).describe("Information about the assigned adjuster"),
  inspection_scheduled: z.boolean().describe("Whether a vehicle inspection has been scheduled"),
  inspection_date: z.string().date().describe("Date of scheduled inspection"),
  timeline: z.array(z.object({
    date: z.string().date().describe("Date of the event"),
    event: z.string().describe("Description of what happened"),
    participant: z.string().describe("Who was involved in this event"),
  })).describe("Timeline of events in the claim process"),
  resolution: z.object({
    approved: z.boolean().describe("Whether the claim was approved"),
  }).describe("Final resolution of the claim"),
});
