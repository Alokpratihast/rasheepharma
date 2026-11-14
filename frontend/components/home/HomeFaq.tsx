import Link from "next/link";
import { ArrowRight, ChevronDown } from "lucide-react";
import { HomeHeading } from "@/components/home/HomeHeading";
import { Container } from "@/components/ui/container";

const questions = [
  {
    question: "What is WHO-GMP certification?",
    answer: "GMP stands for Good Manufacturing Practices. It is the part of quality assurance that ensures medicinal products are consistently produced and controlled to the quality standards appropriate to their intended use.",
  },
  {
    question: "How is Rashe Lifesciences different from big pharma companies?",
    answer: "We follow an efficient business model that focuses on keeping treatments for serious illness available to patients, with the aim of reaching quality medications to all people.",
  },
  {
    question: "How do I enquire about a product or bulk requirement?",
    answer: "Open the B2B enquiry form, choose a product, add the required quantity and share your business contact details. The team can then review the request and respond with next steps.",
  },
  {
    question: "Are all products available in every market?",
    answer: "Availability, documentation and destination requirements are confirmed for each enquiry. A product listing does not guarantee availability in every country.",
  },
  {
    question: "Can I review the company certificates?",
    answer: "The certificates are shown in the company section above. Select a certificate image to open a larger preview.",
  },
];

export function HomeFaq() {
  return (
    <section className="bg-slate-50 py-14 sm:py-20" aria-label="Frequently asked questions">
      <Container>
        <div className="mx-auto max-w-3xl">
          <HomeHeading
            eyebrow="Quick answers"
            title="Before you send an enquiry"
            description="A few details about our company, product availability and working with our team."
          />

          <div className="mt-8 divide-y divide-slate-200 rounded-2xl border border-slate-200 bg-white px-5 sm:px-7">
            {questions.map((item, index) => (
              <details key={item.question} className="group py-5" open={index === 0}>
                <summary className="flex cursor-pointer list-none items-center justify-between gap-4 text-left text-base font-semibold text-brand-dark marker:hidden focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-4">
                  {item.question}
                  <ChevronDown aria-hidden="true" className="size-5 shrink-0 text-primary transition-transform group-open:rotate-180" />
                </summary>
                <p className="max-w-2xl pt-3 pr-8 text-sm leading-6 text-slate-600">{item.answer}</p>
              </details>
            ))}
          </div>

          <Link href="/b2b/enquiry" className="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-primary hover:underline">
            Still have a question? Send an enquiry <ArrowRight className="size-4" />
          </Link>
        </div>
      </Container>
    </section>
  );
}
